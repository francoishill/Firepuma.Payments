using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionAppManager.Services.Results;

namespace Firepuma.PaymentsService.FunctionAppManager.Services;

public interface IFunctionsHostManager
{
    Task<CreateFunctionsHostSecretKeyResult> CreateHostSecretKeyIfNotExists(string keyName, CancellationToken cancellationToken);
}