using Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.TableProviders;

public class PayFastOnceOffPaymentsTableProvider : BaseTableProvider
{
    public PayFastOnceOffPaymentsTableProvider(CloudTable table)
        : base(table)
    {
    }
}