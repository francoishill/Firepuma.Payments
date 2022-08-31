using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.Repositories;
using Firepuma.Payments.Core.PaymentAppConfiguration.Specifications;
using MediatR;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.FunctionAppManager.Queries;

public static class GetAllClientApps
{
    public class Query : IRequest<IEnumerable<PaymentApplicationConfig>>
    {
    }

    public class Handler : IRequestHandler<Query, IEnumerable<PaymentApplicationConfig>>
    {
        private readonly IPaymentApplicationConfigRepository _applicationConfigRepository;

        public Handler(
            IPaymentApplicationConfigRepository applicationConfigRepository)
        {
            _applicationConfigRepository = applicationConfigRepository;
        }

        public async Task<IEnumerable<PaymentApplicationConfig>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var specification = new ClientAppsGetAllSpecification();
            var tableQuery = await _applicationConfigRepository.GetItemsAsync(specification, cancellationToken);

            var rows = new List<PaymentApplicationConfig>();
            foreach (var row in tableQuery)
            {
                rows.Add(row);
            }

            return rows;
        }
    }
}