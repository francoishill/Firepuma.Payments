using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Queues;
using Firepuma.PaymentsService.FunctionAppManager.Services.Results;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.PaymentsService.FunctionAppManager.Services;

public class ClientAppManagerService : IClientAppManagerService
{
    private readonly ILogger<ClientAppManagerService> _logger;
    private readonly IMapper _mapper;

    public ClientAppManagerService(
        ILogger<ClientAppManagerService> logger,
        IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<CreateQueueResult> CreateServiceBusQueueIfNotExists(
        string serviceBusConnectionString,
        string applicationId,
        CancellationToken cancellationToken)
    {
        var parsedConnectionString = ServiceBusConnectionStringProperties.Parse(serviceBusConnectionString);
        var busAdminClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

        var queueName = QueueNameFormatter.GetPaymentUpdatedQueueName(applicationId);
        var authorizationRule = new SharedAccessAuthorizationRule("DefaultFirepumaListenerKey", new[] { AccessRights.Listen });

        QueueProperties queue = null;
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