using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Implementations.Config;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.FunctionAppManager.Queries;

public static class GetAllClientApps
{
    public class Query : IRequest<IEnumerable<PayFastClientAppConfig>>
    {
        public CloudTable CloudTable { get; set; }

        public Query(CloudTable cloudTable)
        {
            CloudTable = cloudTable;
        }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<PayFastClientAppConfig>>
    {
        public async Task<IEnumerable<PayFastClientAppConfig>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var table = query.CloudTable;

            var rows = new List<PayFastClientAppConfig>();
            TableContinuationToken token = null;
            var tableFilter = new TableQuery<PayFastClientAppConfig>();

            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(tableFilter, token, cancellationToken);
                rows.AddRange(queryResult.Results);

                token = queryResult.ContinuationToken;
            } while (token != null);

            return rows;
        }
    }
}