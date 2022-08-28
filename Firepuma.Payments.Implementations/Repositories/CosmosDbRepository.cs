using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Firepuma.Payments.Abstractions.Entities;
using Firepuma.Payments.Abstractions.Repositories;
using Firepuma.Payments.Abstractions.Specifications;
using Firepuma.Payments.Implementations.Specifications;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Firepuma.Payments.Implementations.Repositories;

public abstract class CosmosDbRepository<T> : IRepository<T>, ICosmosContainerContext<T> where T : BaseEntity, new()
{
    private readonly Container _container;

    protected CosmosDbRepository(Container container)
    {
        _container = container;
    }

    public abstract string GenerateId(T entity);
    public abstract PartitionKey ResolvePartitionKey(string entityId);

    public string ContainerName => _container.Id;

    public async Task<IEnumerable<T>> GetItemsAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken)
    {
        var queryable = ApplySpecification(specification);
        var iterator = queryable.ToFeedIterator<T>();

        var results = new List<T>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);

            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<IEnumerable<T>> GetItemsAsync(
        string queryString,
        CancellationToken cancellationToken)
    {
        var resultSetIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
        var results = new List<T>();
        while (resultSetIterator.HasMoreResults)
        {
            var response = await resultSetIterator.ReadNextAsync(cancellationToken);

            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<int> GetItemsCountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken)
    {
        var queryable = ApplySpecification(specification);
        return await queryable.CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<T> GetItemAsync(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, ResolvePartitionKey(id), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task AddItemAsync(
        T item,
        CancellationToken cancellationToken)
    {
        item.Id = GenerateId(item);
        await _container.CreateItemAsync<T>(item, ResolvePartitionKey(item.Id), cancellationToken: cancellationToken);
    }

    public async Task UpdateItemAsync(
        T item,
        CancellationToken cancellationToken)
    {
        await _container.UpsertItemAsync<T>(item, ResolvePartitionKey(item.Id), cancellationToken: cancellationToken);
    }

    public async Task DeleteItemAsync(
        T item,
        CancellationToken cancellationToken)
    {
        await _container.DeleteItemAsync<T>(item.Id, ResolvePartitionKey(item.Id), cancellationToken: cancellationToken);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        var evaluator = new CosmosDbSpecificationEvaluator<T>();
        return evaluator.GetQuery(_container.GetItemLinqQueryable<T>(), specification);
    }
}