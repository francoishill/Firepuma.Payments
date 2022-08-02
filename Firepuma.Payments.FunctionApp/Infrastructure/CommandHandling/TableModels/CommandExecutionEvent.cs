using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels
{
    [DebuggerDisplay("{ToString()}")]
    public class CommandExecutionEvent : TableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CommandId { get; set; }
        public string TypeName { get; set; }
        public string TypeNamespace { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool? Successful { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrack { get; set; }
        public double ExecutionTimeInSeconds { get; set; }
        public double TotalTimeInSeconds { get; set; }
        public DateTime? Updated { get; set; }


        public CommandExecutionEvent(BaseCommand baseCommand)
        {
            PartitionKey = "";
            RowKey = Id;

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