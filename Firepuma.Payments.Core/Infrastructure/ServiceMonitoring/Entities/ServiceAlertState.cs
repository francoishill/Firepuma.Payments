using Firepuma.DatabaseRepositories.Abstractions.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.ValueObjects;
using Newtonsoft.Json.Linq;

namespace Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;

public class ServiceAlertState : BaseEntity
{
    public ServiceAlertType AlertType { get; set; }
    public DateTimeOffset NextCheckTime { get; set; } = DateTimeOffset.MinValue;

    public JObject AlertContext { get; set; } = null!;

    public bool TryCastAlertContextToType<T>(out T alertContext, out string? castError) where T : class
    {
        try
        {
            alertContext = AlertContext.ToObject<T>()!;

            if (alertContext == null)
            {
                throw new InvalidCastException($"Unable to cast AlertContext to type {typeof(T).FullName}");
            }

            castError = null;
            return true;
        }
        catch (Exception exception)
        {
            alertContext = null!;
            castError = exception.Message;
            return false;
        }
    }

    public void SetAlertContext<T>(T alertContext) where T : class
    {
        AlertContext = JObject.FromObject(alertContext);
    }
}