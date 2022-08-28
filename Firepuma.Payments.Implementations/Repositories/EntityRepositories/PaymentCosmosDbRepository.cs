using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.Implementations.Payments.TableModels;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Implementations.Repositories.EntityRepositories;

public class PaymentCosmosDbRepository : CosmosDbRepository<PaymentEntity>, IPaymentRepository
{
    public PaymentCosmosDbRepository(Container container)
        : base(container)
    {
    }

    public override string GenerateId(PaymentEntity entity) => GenerateId(entity.ApplicationId, entity.PaymentId);
    public override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);

    private static string GenerateId(ClientApplicationId applicationId, PaymentId paymentId) => $"{paymentId.Value}:{applicationId}";

    public async Task<PaymentEntity> GetItemOrDefaultAsync(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = GenerateId(applicationId, paymentId);
            var response = await Container.ReadItemAsync<PaymentEntity>(id, ResolvePartitionKey(id), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}