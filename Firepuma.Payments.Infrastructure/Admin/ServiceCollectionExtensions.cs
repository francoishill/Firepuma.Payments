using Firepuma.Payments.Infrastructure.Admin.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.Payments.Infrastructure.Admin;

public static class ServiceCollectionExtensions
{
    public static void AddAdminFeature(
        this IServiceCollection services,
        IConfigurationSection adminConfigSection)
    {
        if (adminConfigSection == null) throw new ArgumentNullException(nameof(adminConfigSection));

        services.AddOptions<AdminOptions>().Bind(adminConfigSection).ValidateDataAnnotations().ValidateOnStart();
    }
}