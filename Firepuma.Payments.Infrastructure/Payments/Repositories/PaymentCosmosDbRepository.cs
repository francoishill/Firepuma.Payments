using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.Repositories;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Infrastructure.CosmosDb;
using Microsoft.Azure.Cosmos;

namespace Firepuma.Payments.Infrastructure.Payments.Repositories;

public class PaymentCosmosDbRepository : CosmosDbRepository<PaymentEntity>, IPaymentRepository
{
    public PaymentCosmosDbRepository(Container container)
        : base(container)
    {
    }

    protected override string GenerateId(PaymentEntity entity) => GenerateId(entity.ApplicationId, entity.PaymentId);
    protected override PartitionKey ResolvePartitionKey(string entityId) => new(entityId.Split(':')[1]);

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