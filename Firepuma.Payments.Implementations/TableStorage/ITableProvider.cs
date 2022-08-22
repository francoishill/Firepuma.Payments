using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.TableStorage;

public interface ITableProvider<in TEntity> where TEntity : ITableEntity
{
    string TableName { get; }

    void CreateTableIfNotExists();

    Task<TDerivedType> GetEntityAsync<TDerivedType>(
        string partitionKey,
        string rowKey,
        IEnumerable<string> select = null,
        CancellationToken cancellationToken = default) where TDerivedType : class, TEntity, new();

    AsyncPageable<TDerivedType> QueryAsync<TDerivedType>(
        Expression<Func<TDerivedType, bool>> filter,
        int? maxPerPage = null,
        IEnumerable<string> select = null,
        CancellationToken cancellationToken = default) where TDerivedType : class, TEntity, new();

    Task<Response> AddEntityAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task<Response> UpdateEntityAsync(
        TEntity entity,
        ETag ifMatch,
        TableUpdateMode mode = TableUpdateMode.Merge,
        CancellationToken cancellationToken = default);
}