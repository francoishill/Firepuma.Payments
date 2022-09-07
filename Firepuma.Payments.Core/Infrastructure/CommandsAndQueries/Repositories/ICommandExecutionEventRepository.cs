using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities;
using Firepuma.Payments.Core.Infrastructure.Repositories;

namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Repositories;

public interface ICommandExecutionEventRepository : IRepository<CommandExecutionEvent>
{
}