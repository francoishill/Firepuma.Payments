using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage;

public abstract class BaseTableProvider : ITableProvider
{
    public CloudTable Table { get; }

    protected BaseTableProvider(CloudTable table)
    {
        Table = table;
    }
}