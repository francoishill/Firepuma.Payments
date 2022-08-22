using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Azure;
using Azure.Data.Tables;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels
{
    [DebuggerDisplay("{ToString()}")]
    public class CommandExecutionEvent : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

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


        public CommandExecutionEvent(BaseCommand baseCommand, string rowKey)
        {
            PartitionKey = "";
            RowKey = rowKey;

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
            return $"{PartitionKey}/{RowKey}/{CommandId}/{TypeNamespace}.{TypeName}";
        }
    }
}