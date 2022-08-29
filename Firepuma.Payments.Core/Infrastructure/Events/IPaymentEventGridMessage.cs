using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;

namespace Firepuma.Payments.Core.Infrastructure.Events;

public interface IPaymentEventGridMessage
{
    ClientApplicationId ApplicationId { get; }
}