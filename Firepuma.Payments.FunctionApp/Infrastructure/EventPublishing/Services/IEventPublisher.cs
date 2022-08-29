using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Events;

namespace Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Services;

public interface IEventPublisher
{
    Task PublishAsync<T>(
        T eventData,
        CancellationToken cancellationToken) where T : IPaymentEventGridMessage;
}