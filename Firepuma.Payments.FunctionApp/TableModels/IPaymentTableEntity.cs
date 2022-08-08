using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.TableModels;

public interface IPaymentTableEntity : ITableEntity
{
    string PaymentId { get; }
    string GatewayTypeId { get; set; }
}