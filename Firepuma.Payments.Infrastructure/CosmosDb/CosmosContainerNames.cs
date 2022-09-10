namespace Firepuma.Payments.Infrastructure.CosmosDb;

public static class CosmosContainerNames
{
    public const string COMMAND_EXECUTIONS = "CommandExecutions";
    public const string AUTHORIZATION_FAILURES = "AuthorizationFailures";
    public const string PAYMENTS = "Payments";
    public const string NOTIFICATION_TRACES = "NotificationTraces";
    public const string APPLICATION_CONFIGS = "ApplicationConfigs";
    public const string DEAD_LETTERED_MESSAGES = "DeadLetteredMessages";
    public const string SERVICE_ALERT_STATE = "ServiceAlertStates";
}