using Firepuma.Payments.Core.Entities;
using Firepuma.Payments.Core.Specifications;

namespace Firepuma.Payments.Core.Repositories;

public interface IRepository<T> where T : BaseEntity, new()
{
    Task<IEnumerable<T>> GetItemsAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken);

    Task<IEnumerable<T>> GetItemsAsync(
        string queryString,
        CancellationToken cancellationToken);

    Task<int> GetItemsCountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken);

    Task<T> GetItemOrDefaultAsync(
        string id,
        CancellationToken cancellationToken);

    Task AddItemAsync(
        T item,
        CancellationToken cancellationToken);

    Task UpdateItemAsync(
        T item,
        CancellationToken cancellationToken);

    Task DeleteItemAsync(
        T item,
        CancellationToken cancellationToken);
}