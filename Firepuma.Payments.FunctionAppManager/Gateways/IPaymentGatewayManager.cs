using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Results.ValueObjects;
using Firepuma.Payments.FunctionAppManager.Gateways.Results;
using Microsoft.AspNetCore.Http;

namespace Firepuma.Payments.FunctionAppManager.Gateways;

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

    Dictionary<string, object> CreatePaymentApplicationConfigExtraValues(object genericRequestDto);
}