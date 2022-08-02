using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.PayFast.TableProviders;

public class PayFastItnTracesTableProvider : BaseTableProvider
{
    public PayFastItnTracesTableProvider(CloudTable table)
        : base(table)
    {
    }
}