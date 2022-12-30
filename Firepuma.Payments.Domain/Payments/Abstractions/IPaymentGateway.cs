using System.Net;
using Firepuma.Payments.Domain.Payments.Abstractions.ExtraValues;
using Firepuma.Payments.Domain.Payments.Abstractions.Results;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Firepuma.Payments.Domain.Payments.Abstractions;

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

    Task<ValidatePrepareRequestResult> ValidatePrepareRequestAsync(
        PreparePaymentRequest preparePaymentRequest,
        CancellationToken cancellationToken);

    Task<string> CreatePaymentEntityExtraValuesAsync(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        IPreparePaymentExtraValues genericExtraValues,
        CancellationToken cancellationToken);

    Task<Uri> CreateRedirectUriAsync(
        PaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        IPreparePaymentExtraValues genericExtraValues,
        string backendNotifyUrl,
        CancellationToken cancellationToken);

    Task<PaymentNotificationRequestResult> DeserializePaymentNotificationRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken);

    Task<ValidatePaymentNotificationResult> ValidatePaymentNotificationAsync(
        PaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        object genericPaymentNotificationPayload,
        IPAddress remoteIp);

    void SetPaymentPropertiesFromNotification(
        PaymentEntity genericPayment,
        BasePaymentNotificationPayload genericPaymentNotificationPayload);
}