using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Domain.Payments.QuerySpecifications;

public class PaymentApplicationConfigByGatewayAndClientAppIdQuerySpecification : QuerySpecification<PaymentApplicationConfig>
{
    public PaymentApplicationConfigByGatewayAndClientAppIdQuerySpecification(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId)
    {
        WhereExpressions.Add(payment =>
            payment.ApplicationId == applicationId
            &&
            payment.GatewayTypeId == gatewayTypeId
        );
    }
}