using Microsoft.AspNetCore.Mvc;

namespace Firepuma.Payments.Worker.Payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        ILogger<PaymentsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{paymentId}")]
    public IActionResult GetPayment(string paymentId)
    {
        //TODO: Cater for the caller having to pass in their ApplicationId

        //TODO: Implement code
        _logger.LogError("TODO: implement GetPayment");
        return Ok();
    }
}