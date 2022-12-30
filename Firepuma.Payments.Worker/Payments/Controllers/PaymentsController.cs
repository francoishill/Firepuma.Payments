using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using Firepuma.Payments.Worker.Payments.Controllers.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Firepuma.Payments.Worker.Payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ILogger<PaymentsController> _logger;
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly IMediator _mediator;
    private readonly IApplicationConfigProvider _applicationConfigProvider;

    public PaymentsController(
        ILogger<PaymentsController> logger,
        IEnumerable<IPaymentGateway> gateways,
        IMediator mediator,
        IApplicationConfigProvider applicationConfigProvider)
    {
        _logger = logger;
        _gateways = gateways;
        _mediator = mediator;
        _applicationConfigProvider = applicationConfigProvider;
    }

    [HttpGet("{paymentId}")]
    public IActionResult GetPayment(string paymentId)
    {
        //TODO: Cater for the caller having to pass in their ApplicationId

        //TODO: Implement code
        _logger.LogError("TODO: implement GetPayment");
        return Ok();
    }

    [HttpPost("{applicationId}/{gatewayTypeId}")]
    public async Task<ActionResult<PreparePaymentResponse>> PreparePayment(
        [FromBody] PreparePaymentRequest prepareRequest,
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken)
    {
        var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

        if (gateway == null)
        {
            _logger.LogError("The payment gateway type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
            return BadRequest($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        if (!gateway.Features.PreparePayment)
        {
            _logger.LogError("Payment gateway \'{GatewayTypeId}\' does not support feature PreparePayment", gatewayTypeId);
            return BadRequest($"Payment gateway '{gatewayTypeId}' does not support feature PreparePayment");
        }

        var applicationConfig = await _applicationConfigProvider.GetApplicationConfigAsync(
            applicationId,
            gatewayTypeId,
            cancellationToken);

        var validateResult = await gateway.ValidatePrepareRequestAsync(prepareRequest, cancellationToken);
        var extraValues = validateResult.ExtraValues;
        var paymentId = prepareRequest.PaymentId;

        var addCommand = new AddPaymentCommand.Payload
        {
            GatewayTypeId = gatewayTypeId,
            ApplicationId = applicationId,
            ApplicationConfig = applicationConfig,
            PaymentId = paymentId,
            ExtraValues = extraValues,
        };

        var result = await _mediator.Send(addCommand, cancellationToken);

        return new PreparePaymentResponse(paymentId, result.RedirectUrl);
    }
}