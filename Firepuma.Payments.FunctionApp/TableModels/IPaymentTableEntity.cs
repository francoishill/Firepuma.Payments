using Firepuma.Payments.Abstractions.ValueObjects;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.TableModels;

public interface IPaymentTableEntity : ITableEntity
{
    PaymentId PaymentId { get; }
}