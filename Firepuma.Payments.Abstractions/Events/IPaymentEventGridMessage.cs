using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.Abstractions.Events;

public interface IPaymentEventGridMessage
{
    ClientApplicationId ApplicationId { get; }
}