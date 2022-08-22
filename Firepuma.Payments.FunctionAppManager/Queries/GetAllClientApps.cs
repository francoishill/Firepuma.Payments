using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.TableStorage;
using MediatR;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.FunctionAppManager.Queries;

public static class GetAllClientApps
{
    public class Query : IRequest<IEnumerable<PayFastClientAppConfig>>
    {
    }

    public class Handler : IRequestHandler<Query, IEnumerable<PayFastClientAppConfig>>
    {
        private readonly ITableProvider<IPaymentApplicationConfig> _applicationConfigsTableProvider;

        public Handler(
            ITableProvider<IPaymentApplicationConfig> applicationConfigsTableProvider)
        {
            _applicationConfigsTableProvider = applicationConfigsTableProvider;
        }

        public async Task<IEnumerable<PayFastClientAppConfig>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var tableQuery = _applicationConfigsTableProvider.QueryAsync<PayFastClientAppConfig>(c => true);

            var rows = new List<PayFastClientAppConfig>();
            await foreach (var row in tableQuery)
            {
                rows.Add(row);
            }

            return rows;
        }
    }
}