using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.PayFast.TableProviders;

public class PayFastOnceOffPaymentsTableProvider : BaseTableProvider
{
    public PayFastOnceOffPaymentsTableProvider(CloudTable table)
        : base(table)
    {
    }
}