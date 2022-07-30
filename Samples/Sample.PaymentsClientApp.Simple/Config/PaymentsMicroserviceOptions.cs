using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618
// ReSharper disable ClassNeverInstantiated.Global

namespace Sample.PaymentsClientApp.Simple.Config;

public class PaymentsMicroserviceOptions
{
    [Required]
    public string ApplicationId { get; set; }

    [Required]
    public string ApplicationSecret { get; set; }

    [Required]
    public string BaseUrl { get; set; }

    public string AuthorizationCode { get; set; }

    [Required]
    public string ServiceBusConnectionString { get; set; }

    [Required]
    public string ServiceBusQueueName { get; set; }

    public int MaxConcurrentCalls { get; set; }
}