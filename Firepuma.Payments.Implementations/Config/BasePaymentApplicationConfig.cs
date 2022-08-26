using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.Implementations.TableStorage;

namespace Firepuma.Payments.Implementations.Config;

public abstract class BasePaymentApplicationConfig : BaseAzureTable
{
    [IgnoreDataMember]
    public PaymentGatewayTypeId GatewayTypeId => new(PartitionKey);

    [IgnoreDataMember]
    public ClientApplicationId ApplicationId => new(RowKey);

    [Required]
    public string ApplicationSecret { get; set; }

    // ReSharper disable once UnusedMember.Global
    protected BasePaymentApplicationConfig(PaymentGatewayTypeId gatewayTypeId)
    {
        // mainly used by Azure Table deserialization (like in GetEntityAsync method)
        PartitionKey = gatewayTypeId.Value;
    }

    protected BasePaymentApplicationConfig(
        PaymentGatewayTypeId gatewayTypeId,
        ClientApplicationId applicationId,
        string applicationSecret)
        : this(gatewayTypeId)
    {
        RowKey = applicationId.Value;

        ApplicationSecret = applicationSecret;
    }
}