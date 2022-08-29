using Firepuma.Payments.Core.Repositories;
using Firepuma.Payments.Implementations.CommandHandling.TableModels;

namespace Firepuma.Payments.Implementations.Repositories.EntityRepositories;

public interface ICommandExecutionEventRepository : IRepository<CommandExecutionEvent>
{
}