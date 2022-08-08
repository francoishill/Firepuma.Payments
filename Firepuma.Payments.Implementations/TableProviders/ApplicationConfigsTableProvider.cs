using Azure.Data.Tables;
using Firepuma.Payments.Implementations.TableStorage;

namespace Firepuma.Payments.Implementations.TableProviders;

public class ApplicationConfigsTableProvider : BaseTableProvider
{
    public ApplicationConfigsTableProvider(TableClient table)
        : base(table)
    {
    }
}