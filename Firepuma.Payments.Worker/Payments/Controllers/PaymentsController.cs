using AutoMapper;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.Queries;
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
    private readonly IMapper _mapper;
    private readonly IApplicationConfigProvider _applicationConfigProvider;

    public PaymentsController(
        ILogger<PaymentsController> logger,
        IEnumerable<IPaymentGateway> gateways,
        IMediator mediator,
        IMapper mapper,
        IApplicationConfigProvider applicationConfigProvider)
    {
        _logger = logger;
        _gateways = gateways;
        _mediator = mediator;
        _mapper = mapper;
        _applicationConfigProvider = applicationConfigProvider;
    }

    [HttpGet("{applicationId}/{paymentId}")]
    public async Task<ActionResult<GetPaymentResponse>> GetPayment(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        CancellationToken cancellationToken)
    {
        var query = new GetPaymentDetailsQuery.Payload
        {
            ApplicationId = applicationId,
            PaymentId = paymentId,
        };

        var payment = await _mediator.Send(query, cancellationToken);

        if (payment.PaymentEntity == null)
        {
            _logger.LogCritical(
                "Unable to load payment for applicationId: {ApplicationId} and paymentId: {PaymentId}, it was null",
                applicationId, paymentId);

            return BadRequest($"Unable to load payment for applicationId: {applicationId} and paymentId: {paymentId}, it was null");
        }

        var gatewayTypeId = payment.PaymentEntity.GatewayTypeId;

        var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

        if (gateway == null)
        {
            return BadRequest($"The payment gateway type '{gatewayTypeId}' is not supported");
        }

        var dto = _mapper.Map<GetPaymentResponse>(payment.PaymentEntity);

        return dto;
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