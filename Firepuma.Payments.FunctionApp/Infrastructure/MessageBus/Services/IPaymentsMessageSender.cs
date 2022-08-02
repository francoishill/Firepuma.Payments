using System.Threading;
using System.Threading.Tasks;

namespace Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Services;

public interface IPaymentsMessageSender
{
    Task SendAsync<T>(
        T messageDto,
        string correlationId,
        CancellationToken cancellationToken) where T : IPaymentBusMessage;
}