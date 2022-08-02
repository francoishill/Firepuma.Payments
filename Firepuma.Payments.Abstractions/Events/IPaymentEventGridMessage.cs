namespace Firepuma.PaymentsService.Abstractions.Events;

public interface IPaymentEventGridMessage
{
    string ApplicationId { get; }
}