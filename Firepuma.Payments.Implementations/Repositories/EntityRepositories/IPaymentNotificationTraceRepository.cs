using Firepuma.Payments.Abstractions.Repositories;
using Firepuma.Payments.Implementations.Payments.TableModels;

namespace Firepuma.Payments.Implementations.Repositories.EntityRepositories;

public interface IPaymentNotificationTraceRepository : IRepository<PaymentNotificationTrace>
{
}