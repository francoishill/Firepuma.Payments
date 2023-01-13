using Firepuma.Payments.Domain.Notifications.Commands;
using Firepuma.Payments.Infrastructure.Admin.Config;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firepuma.Payments.Worker.Admin.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManualHealthCheckingController : ControllerBase
{
    private readonly ILogger<ManualHealthCheckingController> _logger;
    private readonly IOptions<AdminOptions> _adminOptions;
    private readonly IMediator _mediator;

    public ManualHealthCheckingController(
        ILogger<ManualHealthCheckingController> logger,
        IOptions<AdminOptions> adminOptions,
        IMediator mediator)
    {
        _logger = logger;
        _adminOptions = adminOptions;
        _mediator = mediator;
    }

    [HttpPost("write-dummy-error-log")]
    public IActionResult WriteDummyErrorLog()
    {
        _logger.LogError("This is a dummy error log, probably to do a manual health check of error log alerting");
        return Ok("An error log was written");
    }

    [HttpPost("emails")]
    public async Task<IActionResult> TestEmailsMicroservice(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var sendEmailCommand = new SendEmailCommand.Payload
        {
            Subject = $"Dummy email generated from Firepuma Payments service {now:yyyy-MM-dd HH:mm:ss}",
            FromEmail = _adminOptions.Value.FromEmailAddress,
            FromName = _adminOptions.Value.FromName,
            ToEmail = _adminOptions.Value.ToEmailAddress,
            TextBody = $"This is a dummy email generated from Firepuma Payments service, to test the microservice communication with Email Service. It was generated on {now:yyyy-MM-dd HH:mm:ss}",
        };

        await _mediator.Send(sendEmailCommand, cancellationToken);

        return Accepted("Accepted");
    }
}