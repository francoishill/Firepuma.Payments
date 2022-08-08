using Azure.Data.Tables;
using Firepuma.Payments.Implementations.TableStorage;

namespace Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableProviders;

public class CommandExecutionTableProvider : BaseTableProvider
{
    public CommandExecutionTableProvider(TableClient table)
        : base(table)
    {
    }
}