using Azure.Data.Tables;
using Firepuma.Payments.Implementations.TableStorage;

namespace Firepuma.Payments.Implementations.TableProviders;

public class PaymentsTableProvider : BaseTableProvider
{
    public PaymentsTableProvider(TableClient table)
        : base(table)
    {
    }
}