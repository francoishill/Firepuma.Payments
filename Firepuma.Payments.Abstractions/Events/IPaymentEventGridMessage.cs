namespace Firepuma.Payments.Abstractions.Events;

public interface IPaymentEventGridMessage
{
    string ApplicationId { get; }
}