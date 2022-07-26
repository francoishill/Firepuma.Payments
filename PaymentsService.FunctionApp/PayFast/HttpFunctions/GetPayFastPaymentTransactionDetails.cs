using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.Abstractions.DTOs.Responses;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableModels;
using Firepuma.PaymentsService.FunctionApp.PayFast.Validation;
using Firepuma.PaymentsService.Implementations.Config;
using Firepuma.PaymentsService.Implementations.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.HttpFunctions;

public static class GetPayFastPaymentTransactionDetails
{
    [FunctionName("GetPayFastPaymentTransactionDetails")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetPayFastPaymentTransactionDetails/{applicationId}/{paymentId}")] HttpRequest req,
        ILogger log,
        [Table("PaymentsConfigPerApplication", "PayFast", "{applicationId}")] ClientAppConfig clientAppConfig,
        [Table("PayFastOnceOffPayments")] CloudTable paymentsTable,
        string applicationId,
        string paymentId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

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

        var onceOffPayment = await LoadOnceOffPayment(log, paymentsTable, applicationId, paymentId, cancellationToken);
        if (onceOffPayment == null)
        {
            log.LogCritical("Unable to load onceOffPayment for applicationId: {AppId} and paymentId: {PaymentId}, it was null", applicationId, paymentId);
            return HttpResponseFactory.CreateBadRequestResponse($"Unable to load onceOffPayment with applicationId {applicationId} and paymentId {paymentId}");
        }

        var dto = new PayFastOnceOffPaymentResponse(
            onceOffPayment.PaymentId,
            onceOffPayment.EmailAddress,
            onceOffPayment.NameFirst,
            onceOffPayment.ImmediateAmountInRands,
            onceOffPayment.ItemName,
            onceOffPayment.ItemDescription,
            onceOffPayment.Status,
            onceOffPayment.StatusChangedOn);

        return new OkObjectResult(dto);
    }

    private static async Task<PayFastOnceOffPayment> LoadOnceOffPayment(
        ILogger log,
        CloudTable paymentsTable,
        string applicationId,
        string paymentId,
        CancellationToken cancellationToken)
    {
        //TODO: abstract this code method, it is duplicate

        var retrieveOperation = TableOperation.Retrieve<PayFastOnceOffPayment>(applicationId, paymentId);
        var loadResult = await paymentsTable.ExecuteAsync(retrieveOperation, cancellationToken);

        if (loadResult.Result == null)
        {
            log.LogError("loadResult.Result was null for applicationId: {AppId} and paymentId: {PaymentId}", applicationId, paymentId);
            return null;
        }

        return loadResult.Result as PayFastOnceOffPayment;
    }
}