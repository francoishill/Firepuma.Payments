using Firepuma.Payments.Core.ValueObjects;

namespace Firepuma.Payments.Core.Infrastructure.Events;

public interface IPaymentEventGridMessage
{
    ClientApplicationId ApplicationId { get; }
}