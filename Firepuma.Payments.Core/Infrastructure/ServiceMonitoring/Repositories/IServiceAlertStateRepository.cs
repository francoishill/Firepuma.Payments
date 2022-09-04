using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Repositories;

namespace Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;

public interface IServiceAlertStateRepository : IRepository<ServiceAlertState>
{
}