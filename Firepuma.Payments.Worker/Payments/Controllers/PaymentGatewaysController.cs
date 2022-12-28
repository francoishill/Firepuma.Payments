using Microsoft.AspNetCore.Mvc;

namespace Firepuma.Payments.Worker.Payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentGatewaysController : ControllerBase
{
    private readonly ILogger<PaymentGatewaysController> _logger;

    public PaymentGatewaysController(
        ILogger<PaymentGatewaysController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetPaymentGateways()
    {
        _logger.LogError("TODO: implement GetPaymentGateways");
        return Ok();
    }
}