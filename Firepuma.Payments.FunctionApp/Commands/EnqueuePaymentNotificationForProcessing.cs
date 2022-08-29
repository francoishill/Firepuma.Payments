using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Firepuma.Payments.Core.Infrastructure.CommandHandling;
using Firepuma.Payments.Core.Infrastructure.CommandHandling.TableModels.Attributes;
using Firepuma.Payments.Core.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Services;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.Implementations.Config;
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

        [IgnoreCommandAudit] //FIX: this means we won't have history of this PaymentNotificationPayload data, rather add another attribute to write the data to BlobStorage 
        public BasePaymentNotificationPayload PaymentNotificationPayload { get; init; }

        public string RemoteIp { get; init; }
        public string IncomingRequestUri { get; init; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;
            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success()
        {
            return new Result(true, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, reason, errors);
        }

        public enum FailureReason
        {
            UnknownGatewayTypeId,
            ValidationFailed,
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
                return Result.Failed(Result.FailureReason.UnknownGatewayTypeId, $"The payment gateway type '{command.GatewayTypeId}' is not supported");
            }

            var validationResult = await gateway.ValidatePaymentNotificationAsync(
                applicationConfig,
                applicationId,
                paymentNotificationPayload,
                remoteIp);

            if (!validationResult.IsSuccessful)
            {
                return Result.Failed(Result.FailureReason.ValidationFailed, new[] { validationResult.FailedReason.ToString() }.Concat(validationResult.FailedErrors).ToArray());
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

            return Result.Success();
        }
    }
}