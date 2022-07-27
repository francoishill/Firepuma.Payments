using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.Abstractions.DTOs.Requests;
using Firepuma.PaymentsService.Abstractions.DTOs.Responses;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Validation;
using Firepuma.PaymentsService.FunctionApp.PayFast.Factories;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableModels;
using Firepuma.PaymentsService.FunctionApp.PayFast.Validation;
using Firepuma.PaymentsService.Implementations.Config;
using Firepuma.PaymentsService.Implementations.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.HttpFunctions;

public static class PreparePayFastOnceOffPayment
{
    [FunctionName("PreparePayFastOnceOffPayment")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PreparePayFastOnceOffPayment/{applicationId}")] HttpRequest req,
        ILogger log,
        [Table("PaymentsConfigPerApplication", "PayFast", "{applicationId}")] ClientAppConfig clientAppConfig,
        [Table("PayFastOnceOffPayments")] IAsyncCollector<PayFastOnceOffPayment> paymentsCollector,
        string applicationId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var validateAndStoreItnBaseUrl = Environment.GetEnvironmentVariable("FirepumaValidateAndStorePayFastItnBaseUrl");
        if (string.IsNullOrWhiteSpace(validateAndStoreItnBaseUrl))
        {
            return HttpResponseFactory.CreateBadRequestResponse("Environment variable FIREPUMA_PROCESS_PAYFAST_ITN_BASE_URL is required but empty");
        }

        if (!clientAppConfig.ValidateClientAppConfig(applicationId, req.Headers, out var validationStatusCode, out var validationErrors))
        {
            if (validationStatusCode == HttpStatusCode.BadRequest)
            {
                return HttpResponseFactory.CreateBadRequestResponse(validationErrors?.ToArray() ?? new[] { "Validation failed" });
            }

            return new ObjectResult(validationErrors?.ToArray() ?? new[] { "Validation failed" })
            {
                StatusCode = (int)validationStatusCode,
            };
        }

        var validateAndStoreItnUrlWithAppName = AddApplicationIdToItnBaseUrl(validateAndStoreItnBaseUrl, applicationId);

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var requestDTO = JsonConvert.DeserializeObject<PreparePayFastOnceOffPaymentRequest>(requestBody);

        if (requestDTO == null)
        {
            return HttpResponseFactory.CreateBadRequestResponse("Request body is required but empty");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(requestDTO, out var validationResultsForRequest))
        {
            return HttpResponseFactory.CreateBadRequestResponse(new[] { "Request body is invalid" }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage)).ToArray());
        }

        if (requestDTO.SplitPayment != null)
        {
            if (!ValidationHelpers.ValidateDataAnnotations(requestDTO.SplitPayment, out var validationResultsForSplitPayment))
            {
                return HttpResponseFactory.CreateBadRequestResponse(new[] { "SplitPayment is invalid" }.Concat(validationResultsForSplitPayment.Select(s => s.ErrorMessage)).ToArray());
            }
        }

        var paymentId = requestDTO.PaymentId;
        var payment = CreatePayment(applicationId, paymentId, requestDTO);

        await paymentsCollector.AddAsync(payment, cancellationToken);

        var payFastSettings = PayFastSettingsFactory.CreatePayFastSettings(
            clientAppConfig,
            validateAndStoreItnUrlWithAppName,
            payment.PaymentId.Value,
            requestDTO.ReturnUrl,
            requestDTO.CancelUrl);

        var payfastRequest = PayFastRequestFactory.CreateOnceOffPaymentRequest(
            payFastSettings,
            payment.PaymentId,
            requestDTO.BuyerEmailAddress,
            requestDTO.BuyerFirstName,
            requestDTO.ImmediateAmountInRands ?? throw new ArgumentNullException(nameof(requestDTO.ImmediateAmountInRands)),
            requestDTO.ItemName,
            requestDTO.ItemDescription);

        var redirectUrl = PayFastRedirectFactory.CreateRedirectUrl(
            log,
            payFastSettings,
            payfastRequest,
            requestDTO.SplitPayment);

        var response = new PreparePayFastOnceOffPaymentResponse(paymentId, redirectUrl);
        return new OkObjectResult(response);
    }

    private static string AddApplicationIdToItnBaseUrl(string validateAndStoreItnBaseUrl, string applicationId)
    {
        var questionMarkIndex = validateAndStoreItnBaseUrl.IndexOf("?", StringComparison.Ordinal);

        return questionMarkIndex >= 0
            ? validateAndStoreItnBaseUrl.Substring(0, questionMarkIndex).TrimEnd('/') + $"/{applicationId}?{validateAndStoreItnBaseUrl.Substring(questionMarkIndex + 1)}"
            : validateAndStoreItnBaseUrl + $"/{applicationId}";
    }

    private static PayFastOnceOffPayment CreatePayment(
        string applicationId,
        string paymentId,
        PreparePayFastOnceOffPaymentRequest requestDTO)
    {
        var payment = new PayFastOnceOffPayment(
            applicationId,
            paymentId,
            requestDTO.BuyerEmailAddress,
            requestDTO.BuyerFirstName,
            requestDTO.ImmediateAmountInRands ?? throw new ArgumentNullException(nameof(requestDTO.ImmediateAmountInRands)),
            requestDTO.ItemName,
            requestDTO.ItemDescription);
        return payment;
    }
}