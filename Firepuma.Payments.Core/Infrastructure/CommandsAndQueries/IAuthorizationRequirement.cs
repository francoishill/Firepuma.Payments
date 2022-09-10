using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.ValueObjects;
using MediatR;

namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries;

public interface IAuthorizationRequirement : IRequest<AuthorizationResult>
{
}