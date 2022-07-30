using System;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.CommandHandling;

public abstract class BaseCommand
{
    public string CommandId { get; } = Guid.NewGuid().ToString();
    public DateTime CreatedOn { get; } = DateTime.UtcNow;
}