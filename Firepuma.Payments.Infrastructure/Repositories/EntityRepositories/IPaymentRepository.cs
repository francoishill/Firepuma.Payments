using Firepuma.Payments.Core.Repositories;
using Firepuma.Payments.Core.ValueObjects;
using Firepuma.Payments.Infrastructure.Payments.TableModels;

namespace Firepuma.Payments.Infrastructure.Repositories.EntityRepositories;

public interface IPaymentRepository : IRepository<PaymentEntity>
{
    Task<PaymentEntity> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        CancellationToken cancellationToken);
}