using Firepuma.DatabaseRepositories.MongoDb.Repositories;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Firepuma.Payments.Infrastructure.Payments.Repositories;

public class PaymentMongoDbRepository : MongoDbRepository<PaymentEntity>, IPaymentRepository
{
    public PaymentMongoDbRepository(ILogger logger, IMongoCollection<PaymentEntity> collection)
        : base(logger, collection)
    {
    }
}