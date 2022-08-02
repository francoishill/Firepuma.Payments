using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableProviders;

public class CommandExecutionTableProvider : BaseTableProvider
{
    public CommandExecutionTableProvider(CloudTable table)
        : base(table)
    {
    }
}