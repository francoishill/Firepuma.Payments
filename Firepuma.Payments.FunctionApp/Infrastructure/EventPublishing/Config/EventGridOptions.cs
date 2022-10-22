using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;

namespace Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Config;

public class EventGridOptions
{
    public string EventGridEndpoint { get; set; } = null!;
    public string EventGridAccessKey { get; set; } = null!;

    public SubjectFactoryDelegate SubjectFactory { get; set; } = null!;

    public delegate string SubjectFactoryDelegate(ClientApplicationId applicationId);
}