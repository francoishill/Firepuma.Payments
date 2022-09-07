using Firepuma.Payments.Core.Infrastructure.Repositories;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;

namespace Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;

public interface IServiceAlertStateRepository : IRepository<ServiceAlertState>
{
}