using Firepuma.Payments.Core.Entities;

namespace Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;

public class DeadLetteredMessage : BaseEntity
{
    public string MessageId { get; set; }

    public DateTimeOffset EnqueuedTime { get; set; }
    public string EnqueuedYearAndMonth { get; set; }

    public string MessageBody { get; set; }

    public string Subject { get; set; }
    public string ContentType { get; set; }
    public string CorrelationId { get; set; }
    public int DeliveryCount { get; set; }
    public string PartitionKey { get; set; }
    public string SessionId { get; set; }
    public string DeadLetterReason { get; set; }
    public string DeadLetterSource { get; set; }
    public string DeadLetterErrorDescription { get; set; }
    public Dictionary<string, object> ApplicationProperties { get; set; }
}