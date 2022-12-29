using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Firepuma.Payments.Worker.Payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentWebhooksController : ControllerBase
{
    private readonly ILogger<PaymentWebhooksController> _logger;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentWebhooksController(
        ILogger<PaymentWebhooksController> logger,
        IEnumerable<IPaymentGateway> gateways)
    {
        _logger = logger;
        _gateways = gateways;
    }

    [HttpPost("IncomingPaymentNotification/{gatewayTypeId}/{applicationId}")]
    public IActionResult HandleIncomingPaymentNotificationWebhook(
        PaymentGatewayTypeId gatewayTypeId,
        ClientApplicationId applicationId)
    {
        //TODO: Implement code
        _logger.LogError("TODO: implement HandleIncomingPaymentNotificationWebhook");

        var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

        if (gateway == null)
        {
            _logger.LogError("The payment gateway type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
            return BadRequest($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        // var applicationConfig = await _applicationConfigRepository.GetItemOrDefaultAsync(
        //     new ClientApplicationId(applicationId),
        //     new PaymentGatewayTypeId(gatewayTypeId),
        //     cancellationToken);
        //
        // if (applicationConfig == null)
        // {
        //     _logger.LogError("Unable to find applicationConfig for applicationId: {ApplicationId} and gatewayTypeId: {GatewayTypeId}", applicationId, gatewayTypeId);
        //     return HttpResponseFactory.CreateBadRequestResponse($"Unable to find applicationConfig for applicationId: {applicationId} and gatewayTypeId: {gatewayTypeId}");
        // }
        //
        // var remoteIp = GetRemoteIp(log, req);
        // if (remoteIp == null)
        // {
        //     log.LogCritical("The remote ip is required but null");
        //     return HttpResponseFactory.CreateBadRequestResponse("The remote ip is required but null");
        // }
        //
        // var paymentNotificationRequest = await gateway.DeserializePaymentNotificationRequestAsync(req, cancellationToken);
        // if (!paymentNotificationRequest.IsSuccessful)
        // {
        //     _logger.LogError("{Reason}, {Errors}", paymentNotificationRequest.FailedReason.ToString(), string.Join(", ", paymentNotificationRequest.FailedErrors!));
        //     return HttpResponseFactory.CreateBadRequestResponse($"{paymentNotificationRequest.FailedReason.ToString()}, {string.Join(", ", paymentNotificationRequest.FailedErrors!)}");
        // }
        //
        // _logger.LogInformation("Validating PaymentNotification with payload {Payload}", JsonConvert.SerializeObject(paymentNotificationRequest.Result!.PaymentNotificationPayload));
        //
        // var command = new EnqueuePaymentNotificationForProcessing.Command
        // {
        //     CorrelationId = req.HttpContext.TraceIdentifier,
        //     GatewayTypeId = new PaymentGatewayTypeId(gatewayTypeId),
        //     ApplicationId = new ClientApplicationId(applicationId),
        //     ApplicationConfig = applicationConfig,
        //     PaymentNotificationPayload = paymentNotificationRequest.Result.PaymentNotificationPayload,
        //     RemoteIp = remoteIp.ToString(),
        //     IncomingRequestUri = req.GetDisplayUrl(),
        // };
        //
        // try
        // {
        //     await _mediator.Send(command, cancellationToken);
        //
        //     return new OkResult();
        // }
        // catch (CommandException commandException)
        // {
        //     return commandException.CreateResponseMessageResult();
        // }

        return Ok();
    }
}