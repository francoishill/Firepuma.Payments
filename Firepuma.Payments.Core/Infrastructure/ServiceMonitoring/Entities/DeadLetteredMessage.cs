using Firepuma.DatabaseRepositories.Abstractions.Entities;

namespace Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;

public class DeadLetteredMessage : BaseEntity
{
    public string MessageId { get; set; } = null!;

    public DateTimeOffset EnqueuedTime { get; set; }
    public string EnqueuedYearAndMonth { get; set; } = null!;

    public string MessageBody { get; set; } = null!;

    public string Subject { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string CorrelationId { get; set; } = null!;
    public int DeliveryCount { get; set; }
    public string PartitionKey { get; set; } = null!;
    public string SessionId { get; set; } = null!;
    public string DeadLetterReason { get; set; } = null!;
    public string DeadLetterSource { get; set; } = null!;
    public string DeadLetterErrorDescription { get; set; } = null!;
    public Dictionary<string, object> ApplicationProperties { get; set; } = null!;
}