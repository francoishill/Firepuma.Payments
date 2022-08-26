using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions.Results;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.TableStorage;
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

    Task<BasePaymentApplicationConfig> GetApplicationConfigAsync(
        ITableService<BasePaymentApplicationConfig> applicationConfigsTableService,
        ClientApplicationId applicationId,
        CancellationToken cancellationToken);

    Task<ResultContainer<PrepareRequestResult, PrepareRequestFailureReason>> DeserializePrepareRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken);

    Task<IPaymentTableEntity> CreatePaymentTableEntityAsync(
        BasePaymentApplicationConfig applicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        CancellationToken cancellationToken);

    Task<IPaymentTableEntity> GetPaymentDetailsAsync(
        ITableService<IPaymentTableEntity> tableService,
        BasePaymentApplicationConfig applicationConfig,
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken);

    Task<Uri> CreateRedirectUriAsync(
        BasePaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        string backendNotifyUrl,
        CancellationToken cancellationToken);

    Task<ResultContainer<PaymentNotificationRequestResult, PaymentNotificationRequestFailureReason>> DeserializePaymentNotificationRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken);

    Task<ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>> ValidatePaymentNotificationAsync(
        BasePaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        object genericPaymentNotificationPayload,
        IPAddress remoteIp);

    void SetPaymentPropertiesFromNotification(
        IPaymentTableEntity genericPayment,
        BasePaymentNotificationPayload genericPaymentNotificationPayload);
}