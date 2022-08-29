using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Repositories;

namespace Firepuma.Payments.Core.Payments.Repositories;

public interface IPaymentRepository : IRepository<PaymentEntity>
{
    Task<PaymentEntity> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        CancellationToken cancellationToken);
}