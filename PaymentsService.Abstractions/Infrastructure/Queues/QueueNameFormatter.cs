namespace Firepuma.PaymentsService.Abstractions.Infrastructure.Queues;

public static class QueueNameFormatter
{
    public static string GetPaymentUpdatedQueueName(string applicationId)
    {
        return $"payment-updated-{applicationId}";
    }
}