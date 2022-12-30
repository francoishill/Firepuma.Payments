using System.Net;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Firepuma.Payments.Worker.Payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentWebhooksController : ControllerBase
{
    private readonly ILogger<PaymentWebhooksController> _logger;
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly IMediator _mediator;
    private readonly IApplicationConfigProvider _applicationConfigProvider;

    public PaymentWebhooksController(
        ILogger<PaymentWebhooksController> logger,
        IEnumerable<IPaymentGateway> gateways,
        IMediator mediator,
        IApplicationConfigProvider applicationConfigProvider)
    {
        _logger = logger;
        _gateways = gateways;
        _mediator = mediator;
        _applicationConfigProvider = applicationConfigProvider;
    }

    [HttpPost("IncomingNotification/{applicationId}/{gatewayTypeId}")]
    public async Task<IActionResult> HandleIncomingPaymentNotificationWebhook(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken)
    {
        //TODO: Implement code
        _logger.LogError("TODO: implement HandleIncomingPaymentNotificationWebhook");

        var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

        if (gateway == null)
        {
            _logger.LogError("The payment gateway type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
            return BadRequest($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        var applicationConfig = await _applicationConfigProvider.GetApplicationConfigAsync(
            applicationId,
            gatewayTypeId,
            cancellationToken);

        var remoteIp = GetRemoteIp();
        if (remoteIp == null)
        {
            _logger.LogCritical("The remote ip is required but null");
            return BadRequest("The remote ip is required but null");
        }

        var paymentNotificationRequest = await gateway.DeserializePaymentNotificationRequestAsync(Request, cancellationToken);

        _logger.LogInformation("Validating PaymentNotification with payload {Payload}", JsonConvert.SerializeObject(paymentNotificationRequest.PaymentNotificationPayload));

        var enqueuePaymentNotificationCommand = new EnqueuePaymentNotificationForProcessingCommand.Payload
        {
            CorrelationId = Request.HttpContext.TraceIdentifier,
            GatewayTypeId = gatewayTypeId,
            ApplicationId = applicationId,
            ApplicationConfig = applicationConfig,
            PaymentNotificationPayload = paymentNotificationRequest.PaymentNotificationPayload,
            RemoteIp = remoteIp.ToString(),
            IncomingRequestUri = Request.GetDisplayUrl(),
        };

        await _mediator.Send(enqueuePaymentNotificationCommand, cancellationToken);

        return new OkResult();
    }

    private IPAddress? GetRemoteIp()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedForIpString)
            && forwardedForIpString.Any())
        {
            var firstEntry = forwardedForIpString.First() ?? "";
            if (IPAddress.TryParse(firstEntry, out var forwardedForIp))
            {
                return forwardedForIp;
            }

            if (firstEntry.Contains(":"))
            {
                _logger.LogWarning("Did not expect X-Forwarded-For request header '{Header}' to contain ':' character (with port number), but will strip it out", forwardedForIpString!);

                var originalFirstEntry = firstEntry.Substring(0, firstEntry.IndexOf(":", StringComparison.Ordinal));

                if (IPAddress.TryParse(originalFirstEntry, out forwardedForIp))
                {
                    return forwardedForIp;
                }

                _logger.LogWarning(
                    "Found the X-Forwarded-For request header but could not parse its value as an IPAddress. Tried original value '{Original}' and sanitized value '{Sanitized}'",
                    originalFirstEntry, originalFirstEntry);
            }
        }

        return Request.HttpContext.Connection.RemoteIpAddress;
    }
}