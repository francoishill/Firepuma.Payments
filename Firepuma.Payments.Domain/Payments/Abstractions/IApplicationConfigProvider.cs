using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Domain.Payments.Abstractions;

public interface IApplicationConfigProvider
{
    Task<PaymentApplicationConfig> GetApplicationConfigAsync(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken);
}