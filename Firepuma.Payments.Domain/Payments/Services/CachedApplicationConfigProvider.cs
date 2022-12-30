using System.Collections.Concurrent;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.QuerySpecifications;
using Firepuma.Payments.Domain.Payments.Repositories;
using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Domain.Payments.Services;

public class CachedApplicationConfigProvider : IApplicationConfigProvider
{
    private readonly IPaymentApplicationConfigRepository _configRepository;

    private readonly ConcurrentDictionary<string, PaymentApplicationConfig> _cache = new();

    public CachedApplicationConfigProvider(
        IPaymentApplicationConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task<PaymentApplicationConfig> GetApplicationConfigAsync(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{applicationId}/{gatewayTypeId}";

        if (_cache.TryGetValue(cacheKey, out var cachedConfig))
        {
            return cachedConfig;
        }

        var querySpecification = new PaymentApplicationConfigByGatewayAndClientAppIdQuerySpecification(
            applicationId,
            gatewayTypeId);

        var appConfig = await _configRepository.GetItemOrDefaultAsync(querySpecification, cancellationToken);

        if (appConfig == null)
        {
            throw new Exception($"Application config not found for gateway {gatewayTypeId} and application id {applicationId}");
        }

        return _cache.GetOrAdd(cacheKey, appConfig);
    }
}