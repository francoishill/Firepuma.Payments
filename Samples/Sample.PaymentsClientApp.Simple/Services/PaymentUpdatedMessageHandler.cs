using Firepuma.PaymentsService.Abstractions.Events.EventGridMessages;
using Firepuma.PaymentsService.Abstractions.ValueObjects;

// ReSharper disable ClassNeverInstantiated.Global

namespace Sample.PaymentsClientApp.Simple.Services;

public class PaymentUpdatedMessageHandler
{
    private readonly ILogger<PaymentUpdatedMessageHandler> _logger;

    public PaymentUpdatedMessageHandler(
        ILogger<PaymentUpdatedMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandlePaymentUpdated(PayFastPaymentUpdatedEvent dto, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        _logger.LogInformation(
            "Processing payment updated event for payment id '{Id}', status '{Status}', changed on time '{Time}', correlation id '{CorrelationId}'",
            dto.PaymentId.Value, dto.Status.ToString(), dto.StatusChangedOn, dto.CorrelationId);

        switch (dto.Status)
        {
            case PayFastSubscriptionStatus.New:
                _logger.LogWarning(
                    "Did not expected payment status New event, but just ignoring it since nothing needs to change: payment id '{Id}', changed on {Time}",
                    dto.PaymentId, dto.StatusChangedOn);
                break;

            case PayFastSubscriptionStatus.UpToDate:
                //TODO: add logic to mark payment succeeded
                _logger.LogInformation(
                    "TODO: add logic to mark payment succeeded, for payment id '{Id}' and correlation id '{CorrelationId}'", dto.PaymentId, dto.CorrelationId);
                break;

            case PayFastSubscriptionStatus.Cancelled:
                //TODO: add logic to mark payment cancelled/aborted
                _logger.LogInformation(
                    "TODO: add logic to mark payment cancelled/aborted, for payment id '{Id}' and correlation id '{CorrelationId}'", dto.PaymentId, dto.CorrelationId);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(dto.Status));
        }
    }
}