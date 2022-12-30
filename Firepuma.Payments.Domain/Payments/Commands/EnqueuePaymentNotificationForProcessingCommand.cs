using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities.Attributes;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.Domain.Payments.Commands;

public static class EnqueuePaymentNotificationForProcessingCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required string CorrelationId { get; init; } = null!;

        public required ClientApplicationId ApplicationId { get; init; }
        public required PaymentGatewayTypeId GatewayTypeId { get; init; }

        [IgnoreCommandExecution]
        public required PaymentApplicationConfig ApplicationConfig { get; init; } = null!;

        public required BasePaymentNotificationPayload PaymentNotificationPayload { get; init; } = null!;

        public required string RemoteIp { get; init; } = null!;
        public required string IncomingRequestUri { get; init; } = null!;
    }

    public class Result
    {
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
        private readonly ILogger<Handler> _logger;
        private readonly IEnumerable<IPaymentGateway> _gateways;


        public Handler(
            ILogger<Handler> logger,
            IEnumerable<IPaymentGateway> gateways)
        {
            _logger = logger;
            _gateways = gateways;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            // var gatewayTypeId = payload.GatewayTypeId;
            // var applicationConfig = payload.ApplicationConfig;
            // var applicationId = payload.ApplicationId;
            // var paymentNotificationPayload = payload.PaymentNotificationPayload;
            // var remoteIp = IPAddress.Parse(payload.RemoteIp);

            var gateway = _gateways.GetFromTypeIdOrNull(payload.GatewayTypeId);

            if (gateway == null)
            {
                throw new PreconditionFailedException($"The payment gateway type '{payload.GatewayTypeId}' is not supported");
            }

            //TODO: Implement commented code below
            _logger.LogError("Implement commented code below");
            await Task.CompletedTask;

            // var validationResult = await gateway.ValidatePaymentNotificationAsync(
            //     applicationConfig,
            //     applicationId,
            //     paymentNotificationPayload,
            //     remoteIp);
            //
            // if (!validationResult.IsSuccessful)
            // {
            //     throw new PreconditionFailedException(string.Join(
            //         ", ",
            //         new[] { validationResult.FailedReason.ToString() }.Concat(validationResult.FailedErrors).ToArray()));
            // }
            //
            // var paymentId = validationResult.Result!.PaymentId;
            // var paymentStatus = validationResult.Result.PaymentStatus;
            // var gatewayInternalTransactionId = validationResult.Result.GatewayInternalTransactionId;
            //
            // var messageDto = new PaymentNotificationValidatedMessage
            // {
            //     GatewayTypeId = gatewayTypeId,
            //     ApplicationId = applicationId,
            //     PaymentId = paymentId,
            //     GatewayInternalTransactionId = gatewayInternalTransactionId,
            //     PaymentStatus = paymentStatus,
            //     PaymentNotificationPayload = paymentNotificationPayload,
            //     IncomingRequestUri = payload.IncomingRequestUri,
            // };
            //
            // await _paymentsMessageSender.SendAsync(
            //     messageDto,
            //     payload.CorrelationId,
            //     cancellationToken);

            return new Result();
        }
    }
}