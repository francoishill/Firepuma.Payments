using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.TableProviders;

public class PaymentsTableProvider : BaseTableProvider
{
    public PaymentsTableProvider(CloudTable table)
        : base(table)
    {
    }
}