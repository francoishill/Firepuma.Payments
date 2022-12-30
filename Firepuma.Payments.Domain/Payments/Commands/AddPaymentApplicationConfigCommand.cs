using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Repositories;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using MediatR;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Firepuma.Payments.Domain.Payments.Commands;

public static class AddPaymentApplicationConfigCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required ClientApplicationId ApplicationId { get; init; }
        public required PaymentGatewayTypeId GatewayTypeId { get; init; }

        public required Dictionary<string, string> ExtraValues { get; init; } = null!;
    }

    public class Result
    {
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly IPaymentApplicationConfigRepository _applicationConfigRepository;

        public Handler(
            IPaymentApplicationConfigRepository applicationConfigRepository)
        {
            _applicationConfigRepository = applicationConfigRepository;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            var newClientAppConfig = new PaymentApplicationConfig
            {
                GatewayTypeId = payload.GatewayTypeId,
                ApplicationId = payload.ApplicationId,
                ExtraValues = payload.ExtraValues,
            };

            await _applicationConfigRepository.AddItemAsync(newClientAppConfig, cancellationToken);

            return new Result();
        }
    }
}