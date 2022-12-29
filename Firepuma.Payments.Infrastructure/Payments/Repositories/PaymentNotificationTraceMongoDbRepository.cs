using Firepuma.DatabaseRepositories.MongoDb.Repositories;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Firepuma.Payments.Infrastructure.Payments.Repositories;

public class PaymentNotificationTraceMongoDbRepository : MongoDbRepository<PaymentNotificationTrace>, IPaymentNotificationTraceRepository
{
    public PaymentNotificationTraceMongoDbRepository(ILogger logger, IMongoCollection<PaymentNotificationTrace> collection)
        : base(logger, collection)
    {
    }
}