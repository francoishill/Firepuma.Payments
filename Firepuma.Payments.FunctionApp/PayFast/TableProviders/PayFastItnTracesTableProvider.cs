using Azure.Data.Tables;
using Firepuma.Payments.Implementations.TableStorage;

namespace Firepuma.Payments.FunctionApp.PayFast.TableProviders;

public class PayFastItnTracesTableProvider : BaseTableProvider
{
    public PayFastItnTracesTableProvider(TableClient table)
        : base(table)
    {
    }
}