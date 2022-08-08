using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.TableStorage;

public abstract class BaseTableProvider : ITableProvider
{
    public TableClient Table { get; }

    protected BaseTableProvider(TableClient table)
    {
        Table = table;
    }
}