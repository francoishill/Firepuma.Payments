using Firepuma.Payments.Core.Repositories;
using Firepuma.Payments.Core.ValueObjects;
using Firepuma.Payments.Implementations.Config;

namespace Firepuma.Payments.Implementations.Repositories.EntityRepositories;

public interface IPaymentApplicationConfigRepository : IRepository<PaymentApplicationConfig>
{
    Task<PaymentApplicationConfig> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken);
}