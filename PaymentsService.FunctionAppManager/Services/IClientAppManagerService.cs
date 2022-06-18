using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionAppManager.Services.Results;

namespace Firepuma.PaymentsService.FunctionAppManager.Services;

public interface IClientAppManagerService
{
    Task<CreateQueueResult> CreateServiceBusQueueIfNotExists(
        string serviceBusConnectionString,
        string applicationId,
        CancellationToken cancellationToken);
}