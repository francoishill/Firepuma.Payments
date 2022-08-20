using Azure.Data.Tables;
using Firepuma.Payments.Implementations.TableStorage;

namespace Firepuma.Payments.Implementations.TableProviders;

public class PaymentTracesTableProvider : BaseTableProvider
{
    public PaymentTracesTableProvider(TableClient table)
        : base(table)
    {
    }
}