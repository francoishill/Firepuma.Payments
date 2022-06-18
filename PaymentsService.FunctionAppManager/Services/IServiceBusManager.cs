using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionAppManager.Services.Results;

namespace Firepuma.PaymentsService.FunctionAppManager.Services;

public interface IServiceBusManager
{
    Task<CreateQueueResult> CreateQueueIfNotExists(
        string serviceBusConnectionString,
        string queueName,
        CancellationToken cancellationToken);
}