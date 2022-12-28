using System.ComponentModel.DataAnnotations;

namespace Firepuma.Payments.Worker.Plumbing.LocalDevelopment.Config;

public class LocalDevelopmentOptions
{
    [Required]
    public string PubSubPullProjectId { get; set; } = null!;

    [Required]
    public string PubSubPullSubscriptionId { get; set; } = null!;
}