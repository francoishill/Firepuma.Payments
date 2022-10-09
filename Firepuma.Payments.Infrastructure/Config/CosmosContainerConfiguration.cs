// ReSharper disable InconsistentNaming

using Firepuma.CommandsAndQueries.Abstractions.Entities;
using Firepuma.DatabaseRepositories.CosmosDb.Services.Requests;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.Payments.Entities;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Infrastructure.Config;

public static class CosmosContainerConfiguration
{
    public static readonly ContainerSpecification AuthorizationFailures = new()
    {
        ContainerProperties = new ContainerProperties(id: "AuthorizationFailures", partitionKeyPath: $"/{nameof(AuthorizationFailureEvent.PartitionKey)}"),
    };

    public static readonly ContainerSpecification CommandExecutions = new()
    {
        ContainerProperties = new ContainerProperties(id: "CommandExecutions", partitionKeyPath: $"/{nameof(CommandExecutionEvent.PartitionKey)}"),
    };

    public static readonly ContainerSpecification Payments = new()
    {
        ContainerProperties = new ContainerProperties(id: "Payments", partitionKeyPath: $"/{nameof(PaymentEntity.ApplicationId)}"),
    };

    public static readonly ContainerSpecification NotificationTraces = new()
    {
        ContainerProperties = new ContainerProperties(id: "NotificationTraces", partitionKeyPath: $"/{nameof(PaymentNotificationTrace.ApplicationId)}"),
    };

    public static readonly ContainerSpecification ApplicationConfigs = new()
    {
        ContainerProperties = new ContainerProperties(id: "ApplicationConfigs", partitionKeyPath: $"/{nameof(PaymentApplicationConfig.ApplicationId)}"),
    };

    public static readonly ContainerSpecification DeadLetteredMessages = new()
    {
        ContainerProperties = new ContainerProperties(id: "DeadLetteredMessages", partitionKeyPath: $"/{nameof(DeadLetteredMessage.EnqueuedYearAndMonth)}"),
    };

    public static readonly ContainerSpecification ServiceAlertStates = new()
    {
        ContainerProperties = new ContainerProperties(id: "ServiceAlertStates", partitionKeyPath: $"/{nameof(ServiceAlertState.AlertType)}"),
    };

    public static readonly ContainerSpecification[] AllContainers =
    {
        AuthorizationFailures,
        CommandExecutions,
        Payments,
        NotificationTraces,
        ApplicationConfigs,
        DeadLetteredMessages,
        ServiceAlertStates,
    };
}