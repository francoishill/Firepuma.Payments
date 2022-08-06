using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage;
using Microsoft.Azure.Cosmos.Table;

namespace Firepuma.Payments.FunctionApp.TableProviders;

public class ApplicationConfigsTableProvider : BaseTableProvider
{
    public ApplicationConfigsTableProvider(CloudTable table)
        : base(table)
    {
    }
}