using System.ComponentModel.DataAnnotations;

namespace Firepuma.Payments.Infrastructure.Plumbing.IntegrationEvents.Config;

public class IntegrationEventsOptions
{
    [Required]
    public string FirepumaPaymentsWorkerProjectId { get; init; } = null!;

    [Required]
    public string FirepumaPaymentsWorkerTopicId { get; init; } = null!;

    [Required]
    public string NotificationsServiceProjectId { get; init; } = null!;

    [Required]
    public string NotificationsServiceTopicId { get; init; } = null!;
}