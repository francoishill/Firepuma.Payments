using System.Threading;
using System.Threading.Tasks;

namespace Firepuma.PaymentsService.FunctionAppManager.Services;

public interface IClientAppManagerService
{
    Task CreateServiceBusQueueIfNotExists(
        string serviceBusConnectionString,
        string applicationId,
        CancellationToken cancellationToken);
}