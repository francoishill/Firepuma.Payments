using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionApp.PayFast.Commands;
using Firepuma.PaymentsService.FunctionApp.PayFast.Factories;
using Firepuma.PaymentsService.Implementations.Factories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PayFast;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.HttpFunctions;

public class ValidateAndStorePayFastItn
{
    private readonly IMediator _mediator;

    public ValidateAndStorePayFastItn(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("ValidateAndStorePayFastItn")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ValidateAndStorePayFastItn/{applicationId}")] HttpRequest req,
        ILogger log,
        string applicationId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var transactionIdQueryParam = req.Query[PayFastSettingsFactory.TRANSACTION_ID_QUERY_PARAM_NAME];
        if (!string.IsNullOrWhiteSpace(transactionIdQueryParam))
        {
            log.LogInformation("Found the {ParamName} query param from URL with value {TransactionId}", PayFastSettingsFactory.TRANSACTION_ID_QUERY_PARAM_NAME, transactionIdQueryParam);
        }

        if (!req.HasFormContentType)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogCritical("Request body is not form but contained content: {Body}", requestBody);

            return HttpResponseFactory.CreateBadRequestResponse("Invalid content type, expected form data");
        }

        var payFastRequest = ExtractPayFastNotifyOrNull(req.Form);

        if (payFastRequest == null)
        {
            log.LogCritical("The body is null or empty, aborting processing of it");
            return HttpResponseFactory.CreateBadRequestResponse("The body is null or empty");
        }

        var remoteIp = GetRemoteIp(log, req);
        if (remoteIp == null)
        {
            log.LogCritical("The remote ip is required but null");
            return HttpResponseFactory.CreateBadRequestResponse("The remote ip is required but null");
        }

        var command = new EnqueuePayFastItnForProcessing.Command
        {
            CorrelationId = req.HttpContext.TraceIdentifier,
            ApplicationId = applicationId,
            PayFastRequest = payFastRequest,
            RemoteIp = remoteIp.ToString(),
            IncomingRequestUri = req.GetDisplayUrl(),
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccessful)
        {
            log.LogCritical("Command execution was unsuccessful, reason {Reason}, errors {Errors}", result.FailedReason.ToString(), string.Join(", ", result.FailedErrors));

            return HttpResponseFactory.CreateBadRequestResponse($"{result.FailedReason.ToString()}, {string.Join(", ", result.FailedErrors)}");
        }

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