using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Messaging.ServiceBus.Administration;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Queues;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

    public async Task CreateServiceBusQueueIfNotExists(
        string serviceBusConnectionString,
        string applicationId,
        CancellationToken cancellationToken)
    {
        var busAdminClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

        var queueName = QueueNameFormatter.GetPaymentUpdatedQueueName(applicationId);

        if (await busAdminClient.QueueExistsAsync(queueName, cancellationToken))
        {
            var queue = await busAdminClient.GetQueueAsync(queueName, cancellationToken);

            var propertiesToLog = _mapper.Map<QueueOptionsToLog>(queue.Value);
            _logger.LogInformation("Queue '{Name}' already existed with the following properties: {Properties}", queueName, JsonConvert.SerializeObject(propertiesToLog));
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

            var queue = await busAdminClient.CreateQueueAsync(options, cancellationToken);

            var propertiesToLog = _mapper.Map<QueueOptionsToLog>(queue.Value);
            _logger.LogInformation("Queue '{Name}' created with properties: {Properties}", queueName, JsonConvert.SerializeObject(propertiesToLog));
        }
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