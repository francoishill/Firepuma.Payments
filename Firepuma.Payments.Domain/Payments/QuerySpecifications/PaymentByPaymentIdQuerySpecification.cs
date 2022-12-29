using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Domain.Payments.QuerySpecifications;

public class PaymentByPaymentIdQuerySpecification : QuerySpecification<PaymentEntity>
{
    public PaymentByPaymentIdQuerySpecification(ClientApplicationId applicationId, PaymentId paymentId)
    {
        WhereExpressions.Add(payment =>
            payment.ApplicationId == applicationId
            &&
            payment.PaymentId == paymentId
        );
    }
}