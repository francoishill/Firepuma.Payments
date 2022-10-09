using Firepuma.DatabaseRepositories.Abstractions.QuerySpecifications;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;

namespace Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Specifications;

public class AllDeadLetteredMessagesSpecification : QuerySpecification<DeadLetteredMessage>
{
}