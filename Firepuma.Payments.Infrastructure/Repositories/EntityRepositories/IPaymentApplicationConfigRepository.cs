using Firepuma.Payments.Core.Repositories;
using Firepuma.Payments.Core.ValueObjects;
using Firepuma.Payments.Infrastructure.Config;

namespace Firepuma.Payments.Infrastructure.Repositories.EntityRepositories;

public interface IPaymentApplicationConfigRepository : IRepository<PaymentApplicationConfig>
{
    Task<PaymentApplicationConfig> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken);
}