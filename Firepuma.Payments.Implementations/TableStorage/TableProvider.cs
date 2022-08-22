using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.TableStorage;

public sealed class TableProvider<TEntity> : ITableProvider<TEntity> where TEntity : class, ITableEntity
{
    private readonly TableClient _tableClient;

    public string TableName => _tableClient.Name;

    public TableProvider(TableClient tableClient) => _tableClient = tableClient;

    public void CreateTableIfNotExists()
    {
        _tableClient.CreateIfNotExists();
    }

    public async Task<TDerivedType> GetEntityAsync<TDerivedType>(
        string partitionKey,
        string rowKey,
        IEnumerable<string> select = null,
        CancellationToken cancellationToken = default) where TDerivedType : class, TEntity, new()
    {
        var response = await _tableClient.GetEntityAsync<TDerivedType>(partitionKey, rowKey, select, cancellationToken);
        return response.Value;
    }

    public AsyncPageable<TDerivedType> QueryAsync<TDerivedType>(
        Expression<Func<TDerivedType, bool>> filter,
        int? maxPerPage = null,
        IEnumerable<string> select = null,
        CancellationToken cancellationToken = default) where TDerivedType : class, TEntity, new()
    {
        return _tableClient.QueryAsync(filter, maxPerPage, select, cancellationToken);
    }

    public async Task<Response> AddEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return await _tableClient.AddEntityAsync(entity, cancellationToken);
    }

    public async Task<Response> UpdateEntityAsync(
        TEntity entity,
        ETag ifMatch,
        TableUpdateMode mode = TableUpdateMode.Merge,
        CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpdateEntityAsync(entity, ifMatch, mode, cancellationToken);
    }
}