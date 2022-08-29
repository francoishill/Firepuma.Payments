using Firepuma.Payments.Core.ValueObjects;

namespace Firepuma.Payments.Core.Events;

public interface IPaymentEventGridMessage
{
    ClientApplicationId ApplicationId { get; }
}