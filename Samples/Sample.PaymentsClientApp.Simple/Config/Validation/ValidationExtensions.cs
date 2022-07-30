using Microsoft.Extensions.Options;

namespace Sample.PaymentsClientApp.Simple.Config.Validation;

public static class ValidationExtensions
{
    public static T GetValidatedConfig<T>(this IServiceCollection services, IConfigurationSection section) where T : class
    {
        services
            .AddOptions<T>()
            .Bind(section)
            .ValidateDataAnnotations();

        return services.BuildServiceProvider().GetRequiredService<IOptions<T>>().Value;
    }
}