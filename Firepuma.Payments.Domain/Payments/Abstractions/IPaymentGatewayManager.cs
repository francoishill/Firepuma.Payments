using System.Text.Json;
using Firepuma.Payments.Domain.Payments.Services.ServiceResults;
using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Domain.Payments.Abstractions;

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

    Task<CreateClientApplicationRequestResult> DeserializeCreateClientApplicationRequestAsync(
        JsonDocument requestBody,
        CancellationToken cancellationToken);

    Dictionary<string, string> CreatePaymentApplicationConfigExtraValues(object genericRequestDto);
}