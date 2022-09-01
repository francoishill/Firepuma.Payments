using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.ClientDtos.ClientRequests;
using Firepuma.Payments.Core.ClientDtos.ClientRequests.ExtraValues;
using Firepuma.Payments.Core.Gateways.ValueObjects;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Results.ValueObjects;
using Firepuma.Payments.FunctionApp.Gateways.Results;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

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

    Task<ResultContainer<ValidatePrepareRequestResult, ValidatePrepareRequestFailureReason>> ValidatePrepareRequestAsync(
        PreparePaymentRequest preparePaymentRequest,
        CancellationToken cancellationToken);

    Task<JObject> CreatePaymentEntityExtraValuesAsync(
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