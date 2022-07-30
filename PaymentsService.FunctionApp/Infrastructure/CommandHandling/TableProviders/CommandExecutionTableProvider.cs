using Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.CommandHandling.TableProviders;

public class CommandExecutionTableProvider : BaseTableProvider
{
    public CommandExecutionTableProvider(CloudTable table)
        : base(table)
    {
    }
}