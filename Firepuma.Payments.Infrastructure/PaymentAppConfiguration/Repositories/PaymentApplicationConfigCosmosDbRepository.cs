using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Infrastructure.PaymentAppConfiguration.Repositories;

public class PaymentApplicationConfigCosmosDbRepository : CosmosDbRepository<PaymentApplicationConfig>, IPaymentApplicationConfigRepository
{
    public PaymentApplicationConfigCosmosDbRepository(Container container)
        : base(container)
    {
    }

    public override string GenerateId(PaymentApplicationConfig entity) => GenerateId(entity.ApplicationId, entity.GatewayTypeId);
    public override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);

    private static string GenerateId(ClientApplicationId applicationId, PaymentGatewayTypeId gatewayTypeId) => $"{gatewayTypeId.Value}:{applicationId.Value}";

    public async Task<PaymentApplicationConfig> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentGatewayTypeId gatewayTypeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = GenerateId(applicationId, gatewayTypeId);
            var response = await Container.ReadItemAsync<PaymentApplicationConfig>(id, ResolvePartitionKey(id), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}