using Firepuma.Payments.Core.Infrastructure.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;

namespace Firepuma.Payments.Core.Payments.Entities;

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