using System.Net;
using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities.Attributes;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;
using FluentValidation;
using MediatR;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.Domain.Payments.Commands;

public static class ValidatePaymentNotificationCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required string CorrelationId { get; init; }

        public required ClientApplicationId ApplicationId { get; init; }
        public required PaymentGatewayTypeId GatewayTypeId { get; init; }

        [IgnoreCommandExecution]
        public required PaymentApplicationConfig ApplicationConfig { get; init; }

        public required BasePaymentNotificationPayload PaymentNotificationPayload { get; init; }

        public required string RemoteIp { get; init; }
        public required string IncomingRequestUri { get; init; }
    }

    public class Result : BaseIntegrationEventProducingCommandResponse
    {
        public required string CorrelationId { get; init; }

        public required ClientApplicationId ApplicationId { get; init; }
        public required PaymentGatewayTypeId GatewayTypeId { get; init; }

        public required PaymentId PaymentId { get; init; }
        public required string GatewayInternalTransactionId { get; init; }

        public required PaymentStatus PaymentStatus { get; init; }
        public required BasePaymentNotificationPayload PaymentNotificationPayload { get; init; }

        public required string IncomingRequestUri { get; init; }
    }

    public sealed class Validator : AbstractValidator<Payload>
    {
        public Validator()
        {
            RuleFor(x => x.GatewayTypeId.Value).NotEmpty();
            RuleFor(x => x.ApplicationId.Value).NotEmpty();
            RuleFor(x => x.RemoteIp).NotEmpty();
            RuleFor(x => x.IncomingRequestUri).NotEmpty();

            RuleFor(x => x.ApplicationConfig).NotNull();
            RuleFor(x => x.PaymentNotificationPayload).NotNull();
        }
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public Handler(
            IEnumerable<IPaymentGateway> gateways)
        {
            _gateways = gateways;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            var remoteIp = IPAddress.Parse(payload.RemoteIp);

            var gateway = _gateways.GetFromTypeIdOrNull(payload.GatewayTypeId);

            if (gateway == null)
            {
                throw new PreconditionFailedException($"The payment gateway type '{payload.GatewayTypeId}' is not supported");
            }

            var validationResult = await gateway.ValidatePaymentNotificationAsync(
                payload.ApplicationConfig,
                payload.ApplicationId,
                payload.PaymentNotificationPayload,
                remoteIp);

            var paymentId = validationResult.PaymentId;
            var paymentStatus = validationResult.PaymentStatus;
            var gatewayInternalTransactionId = validationResult.GatewayInternalTransactionId;

            return new Result
            {
                CorrelationId = payload.CorrelationId,
                ApplicationId = payload.ApplicationId,
                GatewayTypeId = payload.GatewayTypeId,
                PaymentId = paymentId,
                GatewayInternalTransactionId = gatewayInternalTransactionId,
                PaymentStatus = paymentStatus,
                PaymentNotificationPayload = payload.PaymentNotificationPayload,
                IncomingRequestUri = payload.IncomingRequestUri,
            };
        }
    }
}