using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Results.ValueObjects;
using Firepuma.Payments.FunctionApp.Gateways.Results;
using Microsoft.AspNetCore.Http;

namespace Firepuma.Payments.FunctionApp.Gateways;

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

    Task<ResultContainer<PrepareRequestResult, PrepareRequestFailureReason>> DeserializePrepareRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken);

    Task<Dictionary<string, object>> CreatePaymentEntityExtraValuesAsync(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        CancellationToken cancellationToken);

    Task<Uri> CreateRedirectUriAsync(
        PaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        string backendNotifyUrl,
        CancellationToken cancellationToken);

    Task<ResultContainer<PaymentNotificationRequestResult, PaymentNotificationRequestFailureReason>> DeserializePaymentNotificationRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken);

    Task<ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>> ValidatePaymentNotificationAsync(
        PaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        object genericPaymentNotificationPayload,
        IPAddress remoteIp);

    void SetPaymentPropertiesFromNotification(
        PaymentEntity genericPayment,
        BasePaymentNotificationPayload genericPaymentNotificationPayload);
}