namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries
{
    public abstract class AbstractRequestAuthorizer<TRequest> : IAuthorizer<TRequest>
    {
        private readonly HashSet<IAuthorizationRequirement> _requirements = new();

        public IEnumerable<IAuthorizationRequirement> Requirements => _requirements;

        protected void UseRequirement(IAuthorizationRequirement requirement)
        {
            if (requirement == null) return;
            _requirements.Add(requirement);
        }

        public abstract Task BuildPolicy(TRequest request, CancellationToken cancellationToken);
    }
}