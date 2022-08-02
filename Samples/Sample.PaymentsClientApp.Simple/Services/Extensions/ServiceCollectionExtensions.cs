using Firepuma.Payments.Client;
using Microsoft.Extensions.Azure;
using Sample.PaymentsClientApp.Simple.Config;
using Sample.PaymentsClientApp.Simple.Config.Validation;
using Sample.PaymentsClientApp.Simple.Services;

// ReSharper disable once CheckNamespace
namespace Sample.PaymentsClientApp.Simple.DependencyInjection
{
    public static class PaymentsExtensions
    {
        private static PaymentsMicroserviceOptions AddValidatedPaymentsMicroserviceOptions(IServiceCollection services, IConfiguration configuration)
        {
            return services.GetValidatedConfig<PaymentsMicroserviceOptions>(configuration.GetSection("PaymentsMicroservice"));
        }

        public static IServiceCollection AddPaymentPreparationsFeature(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var paymentsMicroserviceOptions = AddValidatedPaymentsMicroserviceOptions(services, configuration);

            var paymentsBaseUrl = new Uri(paymentsMicroserviceOptions.BaseUrl + (paymentsMicroserviceOptions.BaseUrl.EndsWith("/") ? "" : "/"));
            services.AddPaymentsServiceClient(
                paymentsBaseUrl,
                paymentsMicroserviceOptions.AuthorizationCode,
                paymentsMicroserviceOptions.ApplicationId,
                paymentsMicroserviceOptions.ApplicationSecret);

            return services;
        }

        public static IServiceCollection AddServiceBusBackgroundProcessor(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isDevelopment)
        {
            var paymentsMicroserviceOptions = AddValidatedPaymentsMicroserviceOptions(services, configuration);

            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(paymentsMicroserviceOptions.ServiceBusConnectionString);
            });

            services.AddSingleton<PaymentUpdatedMessageHandler>();
            services.AddHostedService<ServiceBusBackgroundProcessor>();

            return services;
        }
    }
}