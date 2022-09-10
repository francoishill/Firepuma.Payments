namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries;

public interface IAuthorizer<in T>
{
    IEnumerable<IAuthorizationRequirement> Requirements { get; }
    Task BuildPolicy(T instance, CancellationToken cancellationToken);
}