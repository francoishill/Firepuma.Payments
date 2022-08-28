using Firepuma.Payments.Abstractions.Repositories;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.Implementations.Payments.TableModels;

namespace Firepuma.Payments.Implementations.Repositories.EntityRepositories;

public interface IPaymentRepository : IRepository<PaymentEntity>
{
    Task<PaymentEntity> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        CancellationToken cancellationToken);
}