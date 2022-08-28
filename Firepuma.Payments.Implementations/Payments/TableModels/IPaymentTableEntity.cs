using Azure.Data.Tables;

namespace Firepuma.Payments.Implementations.Payments.TableModels;

public interface IPaymentTableEntity : ITableEntity
{
    string PaymentId { get; }
    string GatewayTypeId { get; set; }

    string Status { get; set; }
    DateTime? StatusChangedOn { get; set; }
}