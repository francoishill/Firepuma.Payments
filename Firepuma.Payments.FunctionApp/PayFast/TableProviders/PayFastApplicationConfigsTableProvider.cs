using Azure.Data.Tables;
using Firepuma.Payments.Implementations.TableStorage;

namespace Firepuma.Payments.FunctionApp.PayFast.TableProviders;

public class PayFastApplicationConfigsTableProvider : BaseTableProvider
{
    public PayFastApplicationConfigsTableProvider(TableClient table)
        : base(table)
    {
    }
}