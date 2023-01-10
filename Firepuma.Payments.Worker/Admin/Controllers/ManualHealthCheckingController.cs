using Firepuma.Dtos.Notifications.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.Payments.Infrastructure.Admin.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firepuma.Payments.Worker.Admin.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManualHealthCheckingController : ControllerBase
{
    private readonly ILogger<ManualHealthCheckingController> _logger;
    private readonly IOptions<AdminOptions> _adminOptions;
    private readonly IIntegrationEventEnvelopeFactory _envelopeFactory;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public ManualHealthCheckingController(
        ILogger<ManualHealthCheckingController> logger,
        IOptions<AdminOptions> adminOptions,
        IIntegrationEventEnvelopeFactory envelopeFactory,
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _logger = logger;
        _adminOptions = adminOptions;
        _envelopeFactory = envelopeFactory;
        _integrationEventPublisher = integrationEventPublisher;
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
        var sendEmailRequest = new SendEmailRequest
        {
            Subject = $"Dummy email generated from Firepuma Payments service {now:yyyy-MM-dd HH:mm:ss}",
            FromEmail = _adminOptions.Value.FromEmailAddress,
            ToEmail = _adminOptions.Value.ToEmailAddress,
            TextBody = $"This is a dummy email generated from Firepuma Payments service, to test the microservice communication with Email Service. It was generated on {now:yyyy-MM-dd HH:mm:ss}",
        };

        var integrationEventEnvelope = _envelopeFactory.CreateEnvelopeFromObject(sendEmailRequest);

        await _integrationEventPublisher.SendAsync(integrationEventEnvelope, cancellationToken);

        return Accepted("Accepted");
    }
}