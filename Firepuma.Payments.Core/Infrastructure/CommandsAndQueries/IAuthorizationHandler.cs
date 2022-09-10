using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.ValueObjects;
using MediatR;

namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries
{
    public interface IAuthorizationHandler<TRequest> : IRequestHandler<TRequest, AuthorizationResult>
        where TRequest : IRequest<AuthorizationResult>
    {
    }
}