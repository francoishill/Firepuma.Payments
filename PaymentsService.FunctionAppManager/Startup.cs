using System;
using AutoMapper;
using Firepuma.PaymentsService.FunctionAppManager;
using Firepuma.PaymentsService.FunctionAppManager.Infrastructure.Config;
using Firepuma.PaymentsService.FunctionAppManager.Infrastructure.Constants;
using Firepuma.PaymentsService.FunctionAppManager.Infrastructure.Helpers;
using Firepuma.PaymentsService.FunctionAppManager.Infrastructure.PipelineBehaviors;
using MediatR;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Firepuma.PaymentsService.FunctionAppManager;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        services.AddAutoMapper(typeof(Startup));
        services.BuildServiceProvider().GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();

        var paymentsServiceFunctionsUrl = new Uri(EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("PaymentsServiceFunctionsUrl"));
        var paymentsServiceFunctionsKey = EnvironmentVariableHelpers.GetRequiredEnvironmentVariable("PaymentsServiceFunctionsKey");

        services.Configure<PaymentsServiceOptions>(config =>
        {
            config.FunctionsUrl = paymentsServiceFunctionsUrl;
            config.FunctionsKey = paymentsServiceFunctionsKey;
        });

        services
            .AddHttpClient(HttpClientConstants.PAYMENTS_SERVICE_FUNCTIONS_HTTP_CLIENT_NAME)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = paymentsServiceFunctionsUrl;
                client.Timeout = TimeSpan.FromMinutes(1);
                client.DefaultRequestHeaders.Add("x-functions-key", paymentsServiceFunctionsKey);
            });

        AddMediator(services);
    }

    private static void AddMediator(IServiceCollection services)
    {
        services.AddMediatR(typeof(Startup));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceLogBehavior<,>));
    }
}