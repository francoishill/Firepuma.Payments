using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Validation;
using Firepuma.PaymentsService.FunctionApp.PayFast.DTOs.Events;
using Firepuma.PaymentsService.FunctionApp.PayFast.Factories;
using Firepuma.PaymentsService.Implementations.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PayFast;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.HttpFunctions;

public static class ValidateAndStorePayFastItn
{
    [FunctionName("ValidateAndStorePayFastItn")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ValidateAndStorePayFastItn/{applicationId}")] HttpRequest req,
        ILogger log,
        [Table("PaymentsConfigPerApplication", "PayFast", "{applicationId}")] ClientAppConfig clientAppConfig,
        [ServiceBus("payfast-itn-requests", EntityType = ServiceBusEntityType.Queue, Connection = "FirepumaPaymentsServiceBus")] IAsyncCollector<ServiceBusMessage> itnRequestsCollector,
        string applicationId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        if (clientAppConfig == null)
        {
            return CreateBadRequestResponse($"Config not found for application with id {applicationId}");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(clientAppConfig, out var validationResultsForConfig))
        {
            return CreateBadRequestResponse(new[] { "Application config is invalid" }.Concat(validationResultsForConfig.Select(s => s.ErrorMessage)).ToArray());
        }

        var TODO = "";
        // Include all the Payfast validation (Signature, ValidateMerchantId, ValidateSourceIp)
        // Do ValidateData when `payfastRequest.payment_status == PayFastStatics.CompletePaymentConfirmation`
        // Respond by adding a message to the relevant app/client Service Bus queue, to be consumed by client

        var transactionIdQueryParam = req.Query[PayFastSettingsFactory.TRANSACTION_ID_QUERY_PARAM_NAME];
        if (!string.IsNullOrWhiteSpace(transactionIdQueryParam))
        {
            log.LogInformation("Found the {ParamName} query param from URL with value {TransactionId}", PayFastSettingsFactory.TRANSACTION_ID_QUERY_PARAM_NAME, transactionIdQueryParam);
        }

        if (!req.HasFormContentType)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogCritical("Request body is not form but contained content: {Body}", requestBody);

            return CreateBadRequestResponse("Invalid content type, expected form data");
        }

        var TODO3 = "";
        // Determine what all the relevant info is to send through ServiceBus (by checking what Snaptask uses from the processed PayFast ITN)

        var payFastRequest = ExtractPayFastNotifyOrNull(req.Form);

        if (payFastRequest == null)
        {
            log.LogCritical("The body is null or empty, aborting processing of it");
            return CreateBadRequestResponse("The body is null or empty");
        }

        var remoteIp = GetRemoteIp(log, req);
        if (remoteIp == null)
        {
            log.LogCritical("The remote ip is required but null");
            return CreateBadRequestResponse("The remote ip is required but null");
        }

        payFastRequest.SetPassPhrase(clientAppConfig.PassPhrase);

        var calculatedSignature = payFastRequest.GetCalculatedSignature();
        var signatureIsValid = payFastRequest.signature == calculatedSignature;

        log.LogInformation("PayFast ITN signature valid: {IsValid}", signatureIsValid);
        if (!signatureIsValid)
        {
            log.LogCritical("PayFast ITN signature validation failed");
            return CreateBadRequestResponse("PayFast ITN signature validation failed");
        }

        var subsetOfPayFastSettings = new PayFastSettings
        {
            MerchantId = clientAppConfig.MerchantId,
            MerchantKey = clientAppConfig.MerchantKey,
            PassPhrase = clientAppConfig.PassPhrase,
            ValidateUrl = PayFastSettingsFactory.GetValidateUrl(clientAppConfig.IsSandbox),
        };
        var payfastValidator = new PayFastValidator(subsetOfPayFastSettings, payFastRequest, remoteIp);

        var merchantIdValidationResult = payfastValidator.ValidateMerchantId();
        log.LogInformation(
            "Merchant Id valid: {MerchantIdValidationResult}, merchant id is {RequestMerchantId}",
            merchantIdValidationResult, payFastRequest.merchant_id);
        if (!merchantIdValidationResult)
        {
            log.LogCritical("PayFast ITN merchant id validation failed, merchant id is {MerchantId}", payFastRequest.merchant_id);
            return CreateBadRequestResponse($"PayFast ITN merchant id validation failed, merchant id is {payFastRequest.merchant_id}");
        }

        var ipAddressValidationResult = await payfastValidator.ValidateSourceIp();
        log.LogInformation("Ip Address valid: {IpAddressValidationResult}, remote IP is {RemoteIp}", ipAddressValidationResult, remoteIp);
        if (!ipAddressValidationResult)
        {
            log.LogCritical("PayFast ITN IPAddress validation failed, ip is {RemoteIp}", remoteIp);
            return CreateBadRequestResponse($"PayFast ITN IPAddress validation failed, ip is {remoteIp}");
        }

        // TODO: Currently seems that the data validation only works for success
        if (payFastRequest.payment_status == PayFastStatics.CompletePaymentConfirmation)
        {
            var dataValidationResult = await payfastValidator.ValidateData();
            log.LogInformation("Data Validation Result: {DataValidationResult}", dataValidationResult);
            if (!dataValidationResult)
            {
                log.LogCritical("PayFast ITN data validation failed");
                return CreateBadRequestResponse("PayFast ITN data validation failed");
            }
        }

        if (payFastRequest.payment_status != PayFastStatics.CompletePaymentConfirmation
            && payFastRequest.payment_status != PayFastStatics.CancelledPaymentConfirmation)
        {
            log.LogCritical("Invalid PayFast ITN payment status '{Status}'", payFastRequest.payment_status);
            return CreateBadRequestResponse($"Invalid PayFast ITN payment status '{payFastRequest.payment_status}'");
        }

        var dto = new PayFastPaymentItnValidatedEvent(
            applicationId,
            payFastRequest.m_payment_id,
            payFastRequest,
            req.GetDisplayUrl());

        var messageJson = JsonConvert.SerializeObject(dto);

        var busMessage = new ServiceBusMessage(messageJson)
        {
            CorrelationId = req.HttpContext.TraceIdentifier
        };

        await itnRequestsCollector.AddAsync(busMessage, cancellationToken);
        await itnRequestsCollector.FlushAsync(cancellationToken);

        return new OkResult();
    }

    private static IPAddress GetRemoteIp(ILogger log, HttpRequest req)
    {
        if (req.Headers.TryGetValue("X-Forwarded-For", out var forwardedForIpString)
            && forwardedForIpString.Any())
        {
            var firstEntry = forwardedForIpString.First() ?? "";
            if (IPAddress.TryParse(firstEntry, out var forwardedForIp))
            {
                return forwardedForIp;
            }

            if (firstEntry.Contains(":"))
            {
                log.LogWarning("Did not expect X-Forwarded-For request header '{Header}' to contain ':' character (with port number), but will strip it out", forwardedForIpString);

                var originalFirstEntry = firstEntry.Substring(0, firstEntry.IndexOf(":", StringComparison.Ordinal));

                if (IPAddress.TryParse(originalFirstEntry, out forwardedForIp))
                {
                    return forwardedForIp;
                }

                log.LogWarning(
                    "Found the X-Forwarded-For request header but could not parse its value as an IPAddress. Tried original value '{Original}' and sanitized value '{Sanitized}'",
                    originalFirstEntry, originalFirstEntry);
            }
        }

        return req.HttpContext.Connection.RemoteIpAddress;
    }

    private static IActionResult CreateBadRequestResponse(params string[] errors)
    {
        return new BadRequestObjectResult(new Dictionary<string, object>
        {
            { "Errors", errors }
        });
    }

    private static PayFastNotify ExtractPayFastNotifyOrNull(IFormCollection formCollection)
    {
        // https://github.com/louislewis2/payfast/blob/master/src/PayFast.AspNetCore/PayFastNotifyModelBinder.cs
        if (formCollection == null || formCollection.Count < 1)
        {
            return null;
        }

        var properties = new Dictionary<string, string>();

        foreach (var key in formCollection.Keys)
        {
            formCollection.TryGetValue(key, value: out var value);

            properties.Add(key, value);
        }

        var model = new PayFastNotify();
        model.FromFormCollection(properties);

        return model;
    }
}