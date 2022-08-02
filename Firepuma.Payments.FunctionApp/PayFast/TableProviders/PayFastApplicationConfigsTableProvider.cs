using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.PayFast.TableProviders;

public class PayFastApplicationConfigsTableProvider : BaseTableProvider
{
    public PayFastApplicationConfigsTableProvider(CloudTable table)
        : base(table)
    {
    }
}