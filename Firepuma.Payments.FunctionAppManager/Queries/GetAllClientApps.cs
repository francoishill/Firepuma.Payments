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
    public class Query : IRequest<IEnumerable<BasePaymentApplicationConfig>>
    {
    }

    public class Handler : IRequestHandler<Query, IEnumerable<BasePaymentApplicationConfig>>
    {
        private readonly ITableService<BasePaymentApplicationConfig> _applicationConfigsTableService;

        public Handler(
            ITableService<BasePaymentApplicationConfig> applicationConfigsTableService)
        {
            _applicationConfigsTableService = applicationConfigsTableService;
        }

        public async Task<IEnumerable<BasePaymentApplicationConfig>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            //FIX: remove direct usage of PayFastClientAppConfig here (should support multiple gateways and be gateway agnostic)
            var tableQuery = _applicationConfigsTableService.QueryAsync<PayFastClientAppConfig>(c => true, cancellationToken: cancellationToken);

            var rows = new List<BasePaymentApplicationConfig>();
            await foreach (var row in tableQuery)
            {
                rows.Add(row);
            }

            return rows;
        }
    }
}