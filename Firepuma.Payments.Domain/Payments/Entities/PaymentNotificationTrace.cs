using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using MongoDB.Driver;

namespace Firepuma.Payments.Domain.Payments.Entities;

public class PaymentNotificationTrace : BaseMongoDbEntity
{
    public ClientApplicationId ApplicationId { get; set; }
    public PaymentGatewayTypeId GatewayTypeId { get; set; }

    public string PaymentId { get; set; } = null!;

    public string GatewayInternalTransactionId { get; set; } = null!;
    public string PaymentNotificationJson { get; set; } = null!;
    public string IncomingRequestUri { get; set; } = null!;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // ReSharper disable once UnusedMember.Global
    public PaymentNotificationTrace()
    {
        // used by Mongo deserialization (including the Add methods, like repository.AddItemAsync)
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

    public static IEnumerable<CreateIndexModel<PaymentNotificationTrace>> GetSchemaIndexes()
    {
        return Array.Empty<CreateIndexModel<PaymentNotificationTrace>>();
    }
}