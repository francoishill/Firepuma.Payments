using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Firepuma.Payments.Abstractions.Events.EventGridMessages;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Services;
using Firepuma.Payments.FunctionApp.PayFast.TableModels;
using Firepuma.Payments.FunctionApp.PayFast.TableProviders;
using MediatR;
using Microsoft.Extensions.Logging;
using PayFast;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.PayFast.Commands;

public static class UpdatePayFastOnceOffPaymentStatus
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public string CorrelationId { get; set; }
        public ClientApplicationId ApplicationId { get; set; }
        public PaymentId PaymentId { get; set; }
        public string PaymentStatus { get; set; }
        public string RequestToken { get; set; }
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
            OnceOffPaymentDoesNotExist,
            UnsupportedStatus,
            PreconditionFailed,
        }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly PayFastOnceOffPaymentsTableProvider _payFastOnceOffPaymentsTableProvider;
        private readonly IEventPublisher _eventPublisher;

        public Handler(
            ILogger<Handler> logger,
            PayFastOnceOffPaymentsTableProvider payFastOnceOffPaymentsTableProvider,
            IEventPublisher eventPublisher)
        {
            _logger = logger;
            _payFastOnceOffPaymentsTableProvider = payFastOnceOffPaymentsTableProvider;
            _eventPublisher = eventPublisher;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var applicationId = command.ApplicationId;
            var paymentId = command.PaymentId;
            var paymentStatus = command.PaymentStatus;

            var onceOffPayment = await LoadOnceOffPayment(applicationId, paymentId, cancellationToken);
            if (onceOffPayment == null)
            {
                _logger.LogCritical("Unable to load onceOffPayment for applicationId: {AppId} and paymentId: {PaymentId}, it was null", applicationId, paymentId);
                return Result.Failed(Result.FailureReason.OnceOffPaymentDoesNotExist, $"PayFast OnceOff payment does not exist, applicationId: {applicationId}, paymentId: {paymentId}");
            }

            if (paymentStatus == PayFastStatics.CompletePaymentConfirmation)
            {
                if (string.IsNullOrWhiteSpace(command.RequestToken))
                {
                    _logger.LogWarning("PayFast ITN for paymentId '{PaymentId}' does not have a payfastToken", paymentId);
                }

                onceOffPayment.SetStatus(PayFastSubscriptionStatus.UpToDate);
                onceOffPayment.PayfastPaymentToken = command.RequestToken;
            }
            else if (paymentStatus == PayFastStatics.CancelledPaymentConfirmation)
            {
                onceOffPayment.SetStatus(PayFastSubscriptionStatus.Cancelled);
            }
            else
            {
                return Result.Failed(Result.FailureReason.UnsupportedStatus, $"PayFast status '{paymentStatus}' is not supported");
            }

            try
            {
                await _payFastOnceOffPaymentsTableProvider.Table.UpdateEntityAsync(onceOffPayment, ETag.All, TableUpdateMode.Replace, cancellationToken);
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                _logger.LogCritical(
                    requestFailedException,
                    "Unable to update PayFast OnceOff table for applicationId: {ApplicationId}, paymentId: {PaymentId}, due to ETag mismatch, exception message is: {Exception}",
                    applicationId, paymentId, requestFailedException.Message);

                return Result.Failed(
                    Result.FailureReason.PreconditionFailed,
                    $"Unable to update PayFast OnceOff table for applicationId: {applicationId}, paymentId: {paymentId}, due to ETag mismatch, exception message is: {requestFailedException.Message}");
            }

            await PublishEvent(command, onceOffPayment, cancellationToken);

            return Result.Success();
        }

        private async Task PublishEvent(
            Command command,
            PayFastOnceOffPayment onceOffPayment,
            CancellationToken cancellationToken)
        {
            var status = Enum.Parse<PayFastSubscriptionStatus>(onceOffPayment.Status);

            var eventData = new PayFastPaymentUpdatedEvent
            {
                CorrelationId = command.CorrelationId,
                ApplicationId = new ClientApplicationId(onceOffPayment.ApplicationId),
                PaymentId = new PaymentId(onceOffPayment.PaymentId),
                Status = status,
                StatusChangedOn = onceOffPayment.StatusChangedOn,
            };

            await _eventPublisher.PublishAsync(eventData, cancellationToken);
        }

        private async Task<PayFastOnceOffPayment> LoadOnceOffPayment(
            ClientApplicationId applicationId,
            PaymentId paymentId,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _payFastOnceOffPaymentsTableProvider.Table.GetEntityAsync<PayFastOnceOffPayment>(applicationId.Value, paymentId.Value, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.NotFound)
            {
                _logger.LogError(
                    requestFailedException,
                    "PayFastOnceOffPayment does not exist for applicationId: {AppId} and paymentId: {PaymentId}",
                    applicationId, paymentId);
                return null;
            }
        }
    }
}