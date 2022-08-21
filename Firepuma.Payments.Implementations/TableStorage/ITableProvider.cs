using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.TableStorage;

public interface ITableProvider<TEntity> where TEntity : class, ITableEntity
{
    TableClient Table { get; }
}