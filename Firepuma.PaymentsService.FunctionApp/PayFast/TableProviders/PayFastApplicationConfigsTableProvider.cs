using Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.TableProviders;

public class PayFastApplicationConfigsTableProvider : BaseTableProvider
{
    public PayFastApplicationConfigsTableProvider(CloudTable table)
        : base(table)
    {
    }
}