using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Config;

public class EventGridOptions
{
    public string EventGridEndpoint { get; set; }
    public string EventGridAccessKey { get; set; }

    public SubjectFactoryDelegate SubjectFactory { get; set; }

    public delegate string SubjectFactoryDelegate(ClientApplicationId applicationId);
}