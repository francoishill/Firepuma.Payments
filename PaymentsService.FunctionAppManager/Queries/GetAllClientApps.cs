using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.Implementations.Config;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.PaymentsService.FunctionAppManager.Queries;

public class GetAllClientApps : IRequest<IEnumerable<ClientAppConfig>>
{
    public CloudTable CloudTable { get; set; }

    public GetAllClientApps(CloudTable cloudTable)
    {
        CloudTable = cloudTable;
    }

    public class Handler : IRequestHandler<GetAllClientApps, IEnumerable<ClientAppConfig>>
    {
        public async Task<IEnumerable<ClientAppConfig>> Handle(
            GetAllClientApps query,
            CancellationToken cancellationToken)
        {
            var table = query.CloudTable;

            var rows = new List<ClientAppConfig>();
            TableContinuationToken token = null;
            var tableFilter = new TableQuery<ClientAppConfig>();

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