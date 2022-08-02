using Firepuma.PaymentsService.Abstractions.Constants;
using Firepuma.PaymentsService.Client.Configuration;
using Firepuma.PaymentsService.Client.HttpClient;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Firepuma.PaymentsService.Client;

// ReSharper disable once CheckNamespace
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentsServiceClient(
        this IServiceCollection services,
        Uri paymentsAppBaseUrl,
        string functionsAuthorizationCode,
        string applicationId,
        string applicationSecret,
        Action<PaymentServiceClientOptions>? configureOptions = null)
    {
        if (paymentsAppBaseUrl == null) throw new ArgumentNullException(nameof(paymentsAppBaseUrl));
        if (applicationId == null) throw new ArgumentNullException(nameof(applicationId));

        services.Configure<PaymentServiceClientOptions>(o =>
        {
            o.ApplicationId = applicationId;

            configureOptions?.Invoke(o);
        });

        services
            .AddHttpClient(HttpClientConstants.PAYMENTS_SERVICE_HTTP_CLIENT_NAME)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = paymentsAppBaseUrl;
                client.Timeout = TimeSpan.FromMinutes(1);

                client.DefaultRequestHeaders.Add(PaymentHttpRequestHeaderKeys.APP_SECRET, applicationSecret);

                if (!string.IsNullOrWhiteSpace(functionsAuthorizationCode))
                {
                    client.DefaultRequestHeaders.Add("x-functions-key", functionsAuthorizationCode);
                }
            });

        services.AddSingleton<IPaymentsServiceClient, PaymentsServiceClient>();

        return services;
    }
}