using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.Abstractions.Events;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.EventPublishing.Services;

public interface IEventPublisher
{
    Task PublishAsync<T>(
        T eventData,
        CancellationToken cancellationToken) where T : IPaymentEventGridMessage;
}