using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Attributes;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.FunctionApp.Gateways;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Services;
using FluentValidation;
using MediatR;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.Commands;

public static class EnqueuePaymentNotificationForProcessing
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public string CorrelationId { get; init; }

        public PaymentGatewayTypeId GatewayTypeId { get; init; }

        public ClientApplicationId ApplicationId { get; init; }

        [IgnoreCommandAudit]
        public PaymentApplicationConfig ApplicationConfig { get; init; }

        public BasePaymentNotificationPayload PaymentNotificationPayload { get; init; }

        public string RemoteIp { get; init; }
        public string IncomingRequestUri { get; init; }
    }

    public class Result
    {
    }

    public sealed class Validator : AbstractValidator<Command>
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

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly IPaymentsMessageSender _paymentsMessageSender;


        public Handler(
            IEnumerable<IPaymentGateway> gateways,
            IPaymentsMessageSender paymentsMessageSender)
        {
            _gateways = gateways;
            _paymentsMessageSender = paymentsMessageSender;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var gatewayTypeId = command.GatewayTypeId;
            var applicationConfig = command.ApplicationConfig;
            var applicationId = command.ApplicationId;
            var paymentNotificationPayload = command.PaymentNotificationPayload;
            var remoteIp = IPAddress.Parse(command.RemoteIp);

            var gateway = _gateways.GetFromTypeIdOrNull(command.GatewayTypeId);

            if (gateway == null)
            {
                throw new PreconditionFailedException($"The payment gateway type '{command.GatewayTypeId}' is not supported");
            }

            var validationResult = await gateway.ValidatePaymentNotificationAsync(
                applicationConfig,
                applicationId,
                paymentNotificationPayload,
                remoteIp);

            if (!validationResult.IsSuccessful)
            {
                throw new PreconditionFailedException(string.Join(
                    ", ",
                    new[] { validationResult.FailedReason.ToString() }.Concat(validationResult.FailedErrors).ToArray()));
            }

            var paymentId = validationResult.Result.PaymentId;
            var paymentStatus = validationResult.Result.PaymentStatus;
            var gatewayInternalTransactionId = validationResult.Result.GatewayInternalTransactionId;

            var messageDto = new PaymentNotificationValidatedMessage
            {
                GatewayTypeId = gatewayTypeId,
                ApplicationId = applicationId,
                PaymentId = paymentId,
                GatewayInternalTransactionId = gatewayInternalTransactionId,
                PaymentStatus = paymentStatus,
                PaymentNotificationPayload = paymentNotificationPayload,
                IncomingRequestUri = command.IncomingRequestUri,
            };

            await _paymentsMessageSender.SendAsync(
                messageDto,
                command.CorrelationId,
                cancellationToken);

            return new Result();
        }
    }
}