using Firepuma.DatabaseRepositories.Abstractions.Repositories;
using Firepuma.Payments.Domain.Payments.Entities;

namespace Firepuma.Payments.Domain.Payments.Repositories;

public interface IPaymentApplicationConfigRepository : IRepository<PaymentApplicationConfig>
{
}