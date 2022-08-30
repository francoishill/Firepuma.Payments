using Firepuma.Payments.Core.Entities;
using Firepuma.Payments.Core.Repositories;
using Firepuma.Payments.Core.Specifications;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Firepuma.Payments.Infrastructure.CosmosDb;

public abstract class CosmosDbRepository<T> : IRepository<T> where T : BaseEntity, new()
{
    protected readonly Container Container;

    protected CosmosDbRepository(Container container)
    {
        Container = container;
    }

    public abstract string GenerateId(T entity);
    public abstract PartitionKey ResolvePartitionKey(string entityId);

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
        var resultSetIterator = Container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
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

    public async Task<T> GetItemOrDefaultAsync(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await Container.ReadItemAsync<T>(id, ResolvePartitionKey(id), cancellationToken: cancellationToken);
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
        await Container.CreateItemAsync<T>(item, ResolvePartitionKey(item.Id), cancellationToken: cancellationToken);
    }

    public async Task UpdateItemAsync(
        T item,
        CancellationToken cancellationToken)
    {
        await Container.UpsertItemAsync<T>(item, ResolvePartitionKey(item.Id), cancellationToken: cancellationToken);
    }

    public async Task DeleteItemAsync(
        T item,
        CancellationToken cancellationToken)
    {
        await Container.DeleteItemAsync<T>(item.Id, ResolvePartitionKey(item.Id), cancellationToken: cancellationToken);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        var evaluator = new CosmosDbSpecificationEvaluator<T>();
        return evaluator.GetQuery(Container.GetItemLinqQueryable<T>(), specification);
    }
}