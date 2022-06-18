using AutoMapper;
using Firepuma.PaymentsService.FunctionAppManager;
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

        services.AddAutoMapper(typeof(Startup));
        services.BuildServiceProvider().GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
    }
}