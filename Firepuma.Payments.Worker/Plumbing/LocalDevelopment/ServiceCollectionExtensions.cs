using Firepuma.Payments.Worker.Plumbing.LocalDevelopment.Config;
using Firepuma.Payments.Worker.Plumbing.LocalDevelopment.Services;

namespace Firepuma.Payments.Worker.Plumbing.LocalDevelopment;

public static class ServiceCollectionExtensions
{
    public static void AddLocalDevelopmentServices(
        this IServiceCollection services,
        IConfigurationSection localDevelopmentOptionsConfigSection)
    {
        if (localDevelopmentOptionsConfigSection == null) throw new ArgumentNullException(nameof(localDevelopmentOptionsConfigSection));

        services.AddOptions<LocalDevelopmentOptions>().Bind(localDevelopmentOptionsConfigSection).ValidateDataAnnotations().ValidateOnStart();

        services.AddHostedService<LocalDevStartupOnceOffActionsService>();
        services.AddHostedService<LocalDevelopmentPullPubSubBackgroundService>();
    }
}