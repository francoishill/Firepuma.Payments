using Firepuma.Payments.Core.Infrastructure.CommandHandling.Entities;
using Firepuma.Payments.Core.Repositories;

namespace Firepuma.Payments.Core.Infrastructure.CommandHandling.Repositories;

public interface ICommandExecutionEventRepository : IRepository<CommandExecutionEvent>
{
}