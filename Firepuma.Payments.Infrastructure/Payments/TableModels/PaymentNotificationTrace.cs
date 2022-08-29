using Firepuma.Payments.Core.Entities;
using Firepuma.Payments.Core.ValueObjects;

namespace Firepuma.Payments.Infrastructure.Payments.TableModels;

public class PaymentNotificationTrace : BaseEntity
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public string PaymentId { get; set; }

    public string GatewayInternalTransactionId { get; set; }
    public string PaymentNotificationJson { get; set; }
    public string IncomingRequestUri { get; set; }

    // ReSharper disable once UnusedMember.Global
    public PaymentNotificationTrace()
    {
        // used by Azure Cosmos deserialization (including the Add methods, like repository.AddItemAsync)
    }

    public PaymentNotificationTrace(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        PaymentId paymentId,
        string gatewayInternalTransactionId,
        string paymentNotificationJson,
        string incomingRequestUri)
    {
        ApplicationId = applicationId;
        GatewayTypeId = gatewayTypeId;

        PaymentId = paymentId.Value;
        GatewayInternalTransactionId = gatewayInternalTransactionId;
        PaymentNotificationJson = paymentNotificationJson;
        IncomingRequestUri = incomingRequestUri;
    }
}