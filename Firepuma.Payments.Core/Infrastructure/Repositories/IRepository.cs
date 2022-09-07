using Firepuma.Payments.Core.Infrastructure.Entities;
using Firepuma.Payments.Core.Infrastructure.Specifications;

namespace Firepuma.Payments.Core.Infrastructure.Repositories;

public interface IRepository<T> where T : BaseEntity, new()
{
    Task<IEnumerable<T>> GetItemsAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetItemsAsync(
        string queryString,
        CancellationToken cancellationToken = default);

    Task<int> GetItemsCountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<T> GetItemOrDefaultAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task AddItemAsync(
        T item,
        CancellationToken cancellationToken = default);

    Task UpsertItemAsync(
        T item,
        bool ignoreETag = false,
        CancellationToken cancellationToken = default);

    Task DeleteItemAsync(
        T item,
        bool ignoreETag = false,
        CancellationToken cancellationToken = default);
}