using Firepuma.Payments.Core.Infrastructure.CommandHandling.TableModels;
using Firepuma.Payments.Core.Repositories;

namespace Firepuma.Payments.Core.Infrastructure.CommandHandling.Repositories;

public interface ICommandExecutionEventRepository : IRepository<CommandExecutionEvent>
{
}