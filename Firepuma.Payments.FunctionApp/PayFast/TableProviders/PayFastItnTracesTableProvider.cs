using Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.TableProviders;

public class PayFastItnTracesTableProvider : BaseTableProvider
{
    public PayFastItnTracesTableProvider(CloudTable table)
        : base(table)
    {
    }
}