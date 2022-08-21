using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.TableStorage;

public sealed class TableProvider<TEntity> : ITableProvider<TEntity> where TEntity : class, ITableEntity
{
    public TableClient Table { get; }

    public TableProvider(TableClient table) => Table = table;
}