using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Firepuma.Payments.Core.Payments.Entities;

namespace Firepuma.Payments.Core.Payments.Repositories;

public interface IPaymentNotificationTraceRepository : IRepository<PaymentNotificationTrace>
{
}