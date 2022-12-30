using System.Text.Json;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Firepuma.Payments.Worker.Payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentClientApplicationsController : ControllerBase
{
    private readonly ILogger<PaymentClientApplicationsController> _logger;
    private readonly IMediator _mediator;
    private readonly IEnumerable<IPaymentGatewayManager> _gatewayManagers;

    public PaymentClientApplicationsController(
        ILogger<PaymentClientApplicationsController> logger,
        IMediator mediator,
        IEnumerable<IPaymentGatewayManager> gatewayManagers)
    {
        _logger = logger;
        _mediator = mediator;
        _gatewayManagers = gatewayManagers;
    }

    [HttpPut("{applicationId}/{gatewayTypeId}")]
    public async Task<IActionResult> AddClientApp(
        JsonDocument requestBody,
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken)
    {
        var gatewayManager = _gatewayManagers.GetFromTypeIdOrNull(gatewayTypeId);

        if (gatewayManager == null)
        {
            _logger.LogError("The payment gateway manager type \'{GatewayTypeId}\' is not supported", gatewayTypeId);
            return BadRequest($"The payment gateway manager type '{gatewayTypeId}' is not supported");
        }

        var createClientAppResult = await gatewayManager.DeserializeCreateClientApplicationRequestAsync(requestBody, cancellationToken);

        var newClientAppConfigExtraValues = gatewayManager.CreatePaymentApplicationConfigExtraValues(createClientAppResult.RequestDto);

        var addCommand = new AddPaymentApplicationConfigCommand.Payload
        {
            GatewayTypeId = gatewayTypeId,
            ApplicationId = applicationId,
            ExtraValues = newClientAppConfigExtraValues,
        };

        await _mediator.Send(addCommand, cancellationToken);

        return Ok();
    }
}