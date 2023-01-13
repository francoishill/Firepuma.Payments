using System.ComponentModel.DataAnnotations;

namespace Firepuma.Payments.Infrastructure.Admin.Config;

public class AdminOptions
{
    [Required]
    public string FromEmailAddress { get; set; } = null!;

    [Required]
    public string FromName { get; set; } = null!;

    [Required]
    public string ToEmailAddress { get; set; } = null!;
}