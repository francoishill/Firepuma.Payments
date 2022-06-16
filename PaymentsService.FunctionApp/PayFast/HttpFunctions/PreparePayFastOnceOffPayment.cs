using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.Abstractions.DTOs.Requests;
using Firepuma.PaymentsService.Abstractions.DTOs.Responses;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Validation;
using Firepuma.PaymentsService.FunctionApp.PayFast.Config;
using Firepuma.PaymentsService.FunctionApp.PayFast.Factories;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableModels;
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
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PreparePayFastOnceOffPayment/{applicationId}")]
        HttpRequest req,
        ILogger log,
        [Table("PaymentsConfigPerApplication", "PayFast", "{applicationId}")]
        ClientAppConfig clientAppConfig,
        [Table("PayFastOnceOffPayments")] IAsyncCollector<PayFastOnceOffPayment> paymentsCollector,
        string applicationId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var validateAndStoreItnBaseUrl = Environment.GetEnvironmentVariable("FirepumaValidateAndStorePayFastItnBaseUrl");
        if (string.IsNullOrWhiteSpace(validateAndStoreItnBaseUrl))
        {
            return CreateBadRequestResponse("Environment variable FIREPUMA_PROCESS_PAYFAST_ITN_BASE_URL is required but empty");
        }

        if (clientAppConfig == null)
        {
            return CreateBadRequestResponse($"Config not found for application with id {applicationId}");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(clientAppConfig, out var validationResultsForConfig))
        {
            return CreateBadRequestResponse(new[] { "Application config is invalid" }.Concat(validationResultsForConfig.Select(s => s.ErrorMessage)).ToArray());
        }

        var validateAndStoreItnUrlWithAppName = AddApplicationIdToItnBaseUrl(validateAndStoreItnBaseUrl, applicationId);

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var requestDTO = JsonConvert.DeserializeObject<PreparePayFastOnceOffPaymentRequest>(requestBody);

        if (requestDTO == null)
        {
            return CreateBadRequestResponse("Request body is required but empty");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(requestDTO, out var validationResultsForRequest))
        {
            return CreateBadRequestResponse(new[] { "Request body is invalid" }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage)).ToArray());
        }

        if (requestDTO.SplitPayment != null)
        {
            if (!ValidationHelpers.ValidateDataAnnotations(requestDTO.SplitPayment, out var validationResultsForSplitPayment))
            {
                return CreateBadRequestResponse(new[] { "SplitPayment is invalid" }.Concat(validationResultsForSplitPayment.Select(s => s.ErrorMessage)).ToArray());
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


        var TODO = "";
        // Get the PayfastSettings for the given ApplicationId
        // Insert new record with new ID into PayfastOnceOffPayment table
        // Use PayFastSettingsHelper and PayFastRequestFactory to CreateOnceOffPaymentRequest, pass the new request:
        //    - Payment ID from PayfastOnceOffPayment instance
        //    - ImmediateAmount
        //    - ItemName
        //    - ItemDescription
        //    - ActorName
        //    - ActorEmail
        // Store the DTO in the Abstractions csproj and publish it as a Nuget package
        // Ensure we do validation of PayFastConfigForApplication
        // Ensure we do validation of DataAnnotations on the request DTO
        // Test the validation of SplitPayment nested fields in PreparePayFastOnceOffPaymentRequest
        // Change the BackendNotifyUrl to point to the online Function once it is published (of application config stored in Azure Table ConfigPerApplication)
        // Respond with redirect url, remember to cater for:
        //    - { "split_payment": { "merchant_id": "??", "amount": "??" } }

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

    private static IActionResult CreateBadRequestResponse(params string[] errors)
    {
        return new BadRequestObjectResult(new Dictionary<string, object>
        {
            { "Errors", errors }
        });
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