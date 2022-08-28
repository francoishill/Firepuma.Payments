using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.Implementations.Config;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Implementations.Repositories.EntityRepositories;

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