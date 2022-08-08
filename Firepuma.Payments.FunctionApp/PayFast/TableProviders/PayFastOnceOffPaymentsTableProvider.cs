using Azure.Data.Tables;
using Firepuma.Payments.Implementations.TableStorage;

namespace Firepuma.Payments.FunctionApp.PayFast.TableProviders;

public class PayFastOnceOffPaymentsTableProvider : BaseTableProvider
{
    public PayFastOnceOffPaymentsTableProvider(TableClient table)
        : base(table)
    {
    }
}