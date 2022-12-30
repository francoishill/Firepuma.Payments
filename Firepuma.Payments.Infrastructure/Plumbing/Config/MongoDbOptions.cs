using System.ComponentModel.DataAnnotations;

namespace Firepuma.Payments.Infrastructure.Plumbing.Config;

public class MongoDbOptions
{
    [Required]
    public string ConnectionString { get; set; } = null!;

    [Required]
    public string DatabaseName { get; set; } = null!;

    [Required]
    public string AuthorizationFailuresCollectionName { get; set; } = null!;

    [Required]
    public string CommandExecutionsCollectionName { get; set; } = null!;

    [Required]
    public string AppConfigurationsCollectionName { get; set; } = null!;

    [Required]
    public string PaymentsCollectionName { get; set; } = null!;

    [Required]
    public string NotificationTracesCollectionName { get; set; } = null!;
}