using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionAppManager.GatewayAbstractions.Results;
using Firepuma.Payments.Implementations.Config;
using Microsoft.AspNetCore.Http;

namespace Firepuma.Payments.FunctionAppManager.GatewayAbstractions;

public interface IPaymentGatewayManager
{
    /// <summary>
    /// Unique type ID to distinguish the type during dependency injection
    /// </summary>
    PaymentGatewayTypeId TypeId { get; }

    /// <summary>
    /// The display name that might be showed to a user
    /// </summary>
    string DisplayName { get; }

    Task<ResultContainer<CreateClientApplicationRequestResult, CreateClientApplicationRequestFailureReason>> DeserializeCreateClientApplicationRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken);

    BasePaymentApplicationConfig CreatePaymentApplicationConfig(
        ClientApplicationId applicationId,
        object genericRequestDto,
        string applicationSecret);

    Task<BasePaymentApplicationConfig> GetApplicationConfigAsync(
        ClientApplicationId applicationId,
        CancellationToken cancellationToken);
}