using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Worker.Payments.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Firepuma.Payments.Worker.Payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentGatewaysController : ControllerBase
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewaysController(
        IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    [HttpGet]
    public ActionResult<GetAvailablePaymentGatewaysResponse[]> GetPaymentGateways()
    {
        var gatewayResponses = _gateways
            .Select(g => new GetAvailablePaymentGatewaysResponse
            {
                TypeId = g.TypeId,
                DisplayName = g.DisplayName,
                Features = g.Features,
            })
            .ToArray();

        return gatewayResponses;
    }
}