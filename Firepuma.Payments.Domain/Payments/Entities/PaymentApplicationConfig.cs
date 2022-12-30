using Firepuma.DatabaseRepositories.MongoDb.Abstractions.Entities;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using MongoDB.Driver;

// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.Domain.Payments.Entities;

public class PaymentApplicationConfig : BaseMongoDbEntity
{
    public ClientApplicationId ApplicationId { get; init; }
    public PaymentGatewayTypeId GatewayTypeId { get; init; }

    public Dictionary<string, string> ExtraValues { get; init; } = null!; // can be used to store extra values specific to each payment gateway

    public static IEnumerable<CreateIndexModel<PaymentApplicationConfig>> GetSchemaIndexes()
    {
        return new[]
        {
            new CreateIndexModel<PaymentApplicationConfig>(Builders<PaymentApplicationConfig>.IndexKeys.Combine(
                    Builders<PaymentApplicationConfig>.IndexKeys.Ascending(p => p.GatewayTypeId),
                    Builders<PaymentApplicationConfig>.IndexKeys.Ascending(p => p.ApplicationId)
                ),
                new CreateIndexOptions<PaymentApplicationConfig> { Unique = true }
            ),
        };
    }
}