using System.Diagnostics;
using System.Reflection;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Attributes;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities.Helpers;
using Firepuma.Payments.Core.Infrastructure.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Entities
{
    [DebuggerDisplay("{ToString()}")]
    public class CommandExecutionEvent : BaseEntity
    {
        public string CommandId { get; set; }
        public bool? Successful { get; set; }
        public string TypeName { get; set; }
        public string TypeNamespace { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedOn { get; set; }

        public string Result { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrack { get; set; }
        public double ExecutionTimeInSeconds { get; set; }
        public double TotalTimeInSeconds { get; set; }

        // ReSharper disable once UnusedMember.Global
        public CommandExecutionEvent()
        {
            // used by Azure Cosmos deserialization (including the Add methods, like repository.AddItemAsync)
        }

        public CommandExecutionEvent(BaseCommand baseCommand)
        {
            CommandId = baseCommand.CommandId;
            TypeName = CommandTypeNameHelpers.GetTypeNameExcludingNamespace(baseCommand.GetType());
            TypeNamespace = CommandTypeNameHelpers.GetTypeNamespace(baseCommand.GetType());
            Payload = JsonConvert.SerializeObject(baseCommand, GetCommandPayloadSerializerSettings());
            CreatedOn = baseCommand.CreatedOn;
        }

        private static JsonSerializerSettings GetCommandPayloadSerializerSettings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            jsonSerializerSettings.ContractResolver = new JsonIgnoreAuditingResolver();
            return jsonSerializerSettings;
        }

        private class JsonIgnoreAuditingResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                var ignoreCommandAudit = (IgnoreCommandAuditAttribute)property.AttributeProvider?.GetAttributes(typeof(IgnoreCommandAuditAttribute), true).FirstOrDefault();

                if (ignoreCommandAudit != null)
                {
                    property.Ignored = true;
                }

                return property;
            }
        }

        public override string ToString()
        {
            return $"{Id}/{CommandId}/{TypeNamespace}.{TypeName}";
        }
    }
}