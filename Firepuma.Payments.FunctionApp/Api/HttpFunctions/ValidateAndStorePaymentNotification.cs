using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.FunctionApp.Commands;
using Firepuma.Payments.FunctionApp.Gateways;
using Firepuma.Payments.Infrastructure.HttpResponses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.Payments.FunctionApp.Api.HttpFunctions;

public class ValidateAndStorePaymentNotification
{
    private readonly ILogger<ValidateAndStorePaymentNotification> _logger;
    private readonly IMediator _mediator;
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly IPaymentApplicationConfigRepository _applicationConfigRepository;

    public ValidateAndStorePaymentNotification(
        ILogger<ValidateAndStorePaymentNotification> logger,
        IMediator mediator,
        IEnumerable<IPaymentGateway> gateways,
        IPaymentApplicationConfigRepository applicationConfigRepository)
    {
        _logger = logger;
        _mediator = mediator;
        _gateways = gateways;
        _applicationConfigRepository = applicationConfigRepository;
    }

    [FunctionName("ValidateAndStorePaymentNotification")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ValidateAndStorePaymentNotification/{gatewayTypeId}/{applicationId}")] HttpRequest req,
        ILogger log,
        string gatewayTypeId,
        string applicationId,
        CancellationToken cancellationToken)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var gateway = _gateways.GetFromTypeIdOrNull(new PaymentGatewayTypeId(gatewayTypeId));

        if (gateway == null)
        {
            _logger.LogError("The payment gateway type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
            return HttpResponseFactory.CreateBadRequestResponse($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        var applicationConfig = await _applicationConfigRepository.GetItemOrDefaultAsync(
            new ClientApplicationId(applicationId),
            new PaymentGatewayTypeId(gatewayTypeId),
            cancellationToken);

        if (applicationConfig == null)
        {
            _logger.LogError("Unable to find applicationConfig for applicationId: {ApplicationId} and gatewayTypeId: {GatewayTypeId}", applicationId, gatewayTypeId);
            return HttpResponseFactory.CreateBadRequestResponse($"Unable to find applicationConfig for applicationId: {applicationId} and gatewayTypeId: {gatewayTypeId}");
        }

        var remoteIp = GetRemoteIp(log, req);
        if (remoteIp == null)
        {
            log.LogCritical("The remote ip is required but null");
            return HttpResponseFactory.CreateBadRequestResponse("The remote ip is required but null");
        }

        var paymentNotificationRequest = await gateway.DeserializePaymentNotificationRequestAsync(req, cancellationToken);
        if (!paymentNotificationRequest.IsSuccessful)
        {
            _logger.LogError("{Reason}, {Errors}", paymentNotificationRequest.FailedReason.ToString(), string.Join(", ", paymentNotificationRequest.FailedErrors));
            return HttpResponseFactory.CreateBadRequestResponse($"{paymentNotificationRequest.FailedReason.ToString()}, {string.Join(", ", paymentNotificationRequest.FailedErrors)}");
        }

        _logger.LogInformation("Validating PaymentNotification with payload {Payload}", JsonConvert.SerializeObject(paymentNotificationRequest.Result.PaymentNotificationPayload));

        var command = new EnqueuePaymentNotificationForProcessing.Command
        {
            CorrelationId = req.HttpContext.TraceIdentifier,
            GatewayTypeId = new PaymentGatewayTypeId(gatewayTypeId),
            ApplicationId = new ClientApplicationId(applicationId),
            ApplicationConfig = applicationConfig,
            PaymentNotificationPayload = paymentNotificationRequest.Result.PaymentNotificationPayload,
            RemoteIp = remoteIp.ToString(),
            IncomingRequestUri = req.GetDisplayUrl(),
        };

        try
        {
            await _mediator.Send(command, cancellationToken);

            return new OkResult();
        }
        catch (WrappedRequestException wrappedRequestException)
        {
            return wrappedRequestException.CreateResponseMessageResult();
        }
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
}