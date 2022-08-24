using System.Runtime.Serialization;
using Azure.Data.Tables;
using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.Implementations.Config;

public interface IPaymentApplicationConfig : ITableEntity
{
    [IgnoreDataMember]
    public PaymentGatewayTypeId GatewayTypeId { get; }

    [IgnoreDataMember]
    public ClientApplicationId ApplicationId { get; }

    public string ApplicationSecret { get; set; }
}