using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Firepuma.PaymentsService.FunctionAppManager.Commands.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.PaymentsService.FunctionAppManager.Commands;

public class CreateQueue : IRequest<object>
{
    public string ServiceBusConnectionString { get; set; }
    public string QueueName { get; set; }

    public CreateQueue(
        string serviceBusConnectionString,
        string queueName)
    {
        ServiceBusConnectionString = serviceBusConnectionString;
        QueueName = queueName;
    }


    public class Handler : IRequestHandler<CreateQueue, object>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;

        public Handler(
            ILogger<Handler> logger,
            IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<object> Handle(CreateQueue command, CancellationToken cancellationToken)
        {
            var serviceBusConnectionString = command.ServiceBusConnectionString;
            var queueName = command.QueueName;

            var parsedConnectionString = ServiceBusConnectionStringProperties.Parse(serviceBusConnectionString);
            var busAdminClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

            var authorizationRule = new SharedAccessAuthorizationRule("DefaultFirepumaListenerKey", new[] { AccessRights.Listen });

            QueueProperties queue;
            var createResult = new CreateQueueResult
            {
                QueueName = queueName,
            };

            if (await busAdminClient.QueueExistsAsync(queueName, cancellationToken))
            {
                queue = await busAdminClient.GetQueueAsync(queueName, cancellationToken);

                var propertiesToLog = _mapper.Map<QueueOptionsToLog>(queue);
                _logger.LogInformation("Queue '{Name}' already existed with the following properties: {Properties}", queueName, JsonConvert.SerializeObject(propertiesToLog));

                createResult.IsNew = false;
                createResult.QueueProperties = propertiesToLog;
            }
            else
            {
                _logger.LogInformation("No queue found with name '{Name}', will now create it", queueName);

                var options = new CreateQueueOptions(queueName)
                {
                    MaxSizeInMegabytes = 5120,
                    MaxDeliveryCount = 10,
                    DefaultMessageTimeToLive = TimeSpan.FromDays(14),
                    LockDuration = TimeSpan.FromMinutes(1),
                    DeadLetteringOnMessageExpiration = true,
                };

                queue = await busAdminClient.CreateQueueAsync(options, cancellationToken);

                var propertiesToLog = _mapper.Map<QueueOptionsToLog>(queue);
                _logger.LogInformation("Queue '{Name}' created with properties: {Properties}", queueName, JsonConvert.SerializeObject(propertiesToLog));

                createResult.IsNew = true;
                createResult.QueueProperties = propertiesToLog;
            }

            if (queue.AuthorizationRules.FirstOrDefault(rule =>
                    rule.KeyName == authorizationRule.KeyName
                    && authorizationRule.Rights.All(right => rule.Rights.Contains(right))) is SharedAccessAuthorizationRule existingAuthorizationRule)
            {
                authorizationRule = existingAuthorizationRule;
            }
            else
            {
                queue.AuthorizationRules.Add(authorizationRule);

                queue = await busAdminClient.UpdateQueueAsync(queue, cancellationToken);
            }

            createResult.ConnectionString = $"Endpoint=sb://{parsedConnectionString.FullyQualifiedNamespace}/;SharedAccessKeyName={authorizationRule.KeyName};SharedAccessKey={authorizationRule.PrimaryKey};EntityPath={queueName}";

            return createResult;
        }

        [AutoMap(typeof(QueueProperties))]
        private class QueueOptionsToLog
        {
            public long MaxSizeInMegabytes { get; set; }
            public int MaxDeliveryCount { get; set; }
            public TimeSpan DefaultMessageTimeToLive { get; set; }
            public TimeSpan LockDuration { get; set; }
            public bool DeadLetteringOnMessageExpiration { get; set; }

            public bool EnablePartitioning { get; set; }
        }
    }
}