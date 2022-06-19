using System;
using AutoMapper;
using Firepuma.PaymentsService.FunctionAppManager;
using Firepuma.PaymentsService.FunctionAppManager.Constants;
using Firepuma.PaymentsService.FunctionAppManager.Helpers;
using Firepuma.PaymentsService.FunctionAppManager.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Firepuma.PaymentsService.FunctionAppManager;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<IServiceBusManager, ServiceBusManager>();
        services.AddSingleton<IFunctionsHostManager, FunctionsHostManager>();

        services.AddAutoMapper(typeof(Startup));
        services.BuildServiceProvider().GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();

        var paymentsServiceFunctionsUrl = new Uri(EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("PaymentsServiceFunctionsUrl"));
        var paymentsServiceFunctionsKey = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("PaymentsServiceFunctionsKey");
        services
            .AddHttpClient(HttpClientConstants.PAYMENTS_SERVICE_FUNCTIONS_HTTP_CLIENT_NAME)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = paymentsServiceFunctionsUrl;
                client.Timeout = TimeSpan.FromMinutes(1);
                client.DefaultRequestHeaders.Add("x-functions-key", paymentsServiceFunctionsKey);
            });
    }
}