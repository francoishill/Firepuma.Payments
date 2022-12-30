using Firepuma.DatabaseRepositories.MongoDb.Repositories;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Firepuma.Payments.Infrastructure.Payments.Repositories;

public class PaymentApplicationConfigMongoDbRepository : MongoDbRepository<PaymentApplicationConfig>, IPaymentApplicationConfigRepository
{
    public PaymentApplicationConfigMongoDbRepository(ILogger logger, IMongoCollection<PaymentApplicationConfig> collection)
        : base(logger, collection)
    {
    }
}