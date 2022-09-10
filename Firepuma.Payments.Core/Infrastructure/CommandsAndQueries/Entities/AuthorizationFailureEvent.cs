using System.Diagnostics;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities.Helpers;
using Firepuma.Payments.Core.Infrastructure.Entities;

namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities;

[DebuggerDisplay("{ToString()}")]
public class AuthorizationFailureEvent : BaseEntity
{
    public string ActionTypeName { get; set; }
    public string ActionTypeNamespace { get; set; }
    public object ActionPayload { get; set; }
    public FailedRequirement[] FailedRequirements { get; set; }

    // ReSharper disable once UnusedMember.Global
    public AuthorizationFailureEvent()
    {
        // used by Azure Cosmos deserialization (including the Add methods, like repository.AddItemAsync)
    }

    public AuthorizationFailureEvent(
        Type actionType,
        object actionPayload,
        FailedRequirement[] failedRequirements)
    {
        ActionTypeName = CommandTypeNameHelpers.GetTypeNameExcludingNamespace(actionType);
        ActionTypeNamespace = CommandTypeNameHelpers.GetTypeNamespace(actionType);
        ActionPayload = actionPayload;
        FailedRequirements = failedRequirements;
    }

    public class FailedRequirement
    {
        public string TypeName { get; set; }
        public string TypeNamespace { get; set; }
        public object Payload { get; set; }
        public string Message { get; set; }

        // ReSharper disable once UnusedMember.Global
        public FailedRequirement()
        {
            // used by Azure Cosmos deserialization (including the Add methods, like repository.AddItemAsync)
        }

        public FailedRequirement(IAuthorizationRequirement requirement, string message)
        {
            var type = requirement.GetType();
            TypeName = CommandTypeNameHelpers.GetTypeNameExcludingNamespace(type);
            TypeNamespace = CommandTypeNameHelpers.GetTypeNamespace(type);
            Payload = requirement;
            Message = message;
        }
    }

    public override string ToString()
    {
        return $"{Id}/{ActionTypeName}/{ActionTypeNamespace}";
    }
}