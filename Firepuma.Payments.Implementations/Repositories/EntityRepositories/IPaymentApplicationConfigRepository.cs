using Firepuma.Payments.Abstractions.Repositories;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.Implementations.Config;

namespace Firepuma.Payments.Implementations.Repositories.EntityRepositories;

public interface IPaymentApplicationConfigRepository : IRepository<PaymentApplicationConfig>
{
    Task<PaymentApplicationConfig> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken);
}