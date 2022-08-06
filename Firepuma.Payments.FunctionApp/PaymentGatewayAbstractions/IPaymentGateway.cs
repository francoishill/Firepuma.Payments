using System;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions.Results;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.FunctionApp.TableProviders;
using Firepuma.Payments.Implementations.Config;
using Microsoft.AspNetCore.Http;

namespace Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;

public interface IPaymentGateway
{
    /// <summary>
    /// Unique type ID to distinguish the type during dependency injection
    /// </summary>
    PaymentGatewayTypeId TypeId { get; }

    /// <summary>
    /// The display name that might be showed to a user
    /// </summary>
    string DisplayName { get; }

    PaymentGatewayFeatures Features { get; }

    Task<IPaymentApplicationConfig> GetApplicationConfigAsync(
        ApplicationConfigsTableProvider applicationConfigsTableProvider,
        ClientApplicationId applicationId,
        CancellationToken cancellationToken);

    Task<ResultContainer<PrepareRequestResult, PrepareRequestFailureReason>> DeserializePrepareRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken);

    Task<IPaymentTableEntity> CreatePaymentTableEntity(
        IPaymentApplicationConfig applicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        CancellationToken cancellationToken);

    Task<Uri> CreateRedirectUri(
        IPaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        CancellationToken cancellationToken);
}