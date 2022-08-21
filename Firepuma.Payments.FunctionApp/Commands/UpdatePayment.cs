﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Firepuma.Payments.Abstractions.Events.EventGridMessages;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
using Firepuma.Payments.FunctionApp.Infrastructure.EventPublishing.Services;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.FunctionApp.Queries;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.FunctionApp.TableModels.Extensions;
using Firepuma.Payments.Implementations.TableProviders;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.Commands;

public static class UpdatePayment
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public PaymentGatewayTypeId GatewayTypeId { get; init; }

        public ClientApplicationId ApplicationId { get; init; }

        public PaymentId PaymentId { get; init; }
        public string GatewayInternalTransactionId { get; init; }

        public PaymentStatus PaymentStatus { get; set; }

        [IgnoreCommandAudit] //FIX: this means we won't have history of this PaymentNotificationPayload data, rather add another attribute to write the data to BlobStorage
        public BasePaymentNotificationPayload PaymentNotificationPayload { get; init; }

        public string IncomingRequestUri { get; init; }
        public string CorrelationId { get; set; }
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
            UnableToLoadPayment,
            ConflictUpdatingTableEntity,
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly PaymentTracesTableProvider _paymentTracesTableProvider;
        private readonly PaymentsTableProvider _paymentsTableProvider;
        private readonly IMediator _mediator;
        private readonly IEventPublisher _eventPublisher;

        public Handler(
            ILogger<Handler> logger,
            IEnumerable<IPaymentGateway> gateways,
            PaymentTracesTableProvider paymentTracesTableProvider,
            PaymentsTableProvider paymentsTableProvider,
            IMediator mediator,
            IEventPublisher eventPublisher)
        {
            _logger = logger;
            _gateways = gateways;
            _paymentTracesTableProvider = paymentTracesTableProvider;
            _paymentsTableProvider = paymentsTableProvider;
            _mediator = mediator;
            _eventPublisher = eventPublisher;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var gatewayTypeId = command.GatewayTypeId;
            var applicationId = command.ApplicationId;
            var paymentId = command.PaymentId;
            var paymentStatus = command.PaymentStatus;

            _logger.LogInformation(
                "Updating payment Id {PaymentId} (gateway {GatewayTypeId}, applicationId {ApplicationId}) with status \'{S}\'",
                paymentId, gatewayTypeId, applicationId, paymentStatus.ToString());

            var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

            if (gateway == null)
            {
                return Result.Failed(Result.FailureReason.UnknownGatewayTypeId, $"The payment gateway type '{command.GatewayTypeId}' is not supported");
            }

            try
            {
                //FIX: this could fail if request is large and does not fit into string field of Azure Table, consider writing to blob instead?
                var paymentNotificationJson = JsonConvert.SerializeObject(command.PaymentNotificationPayload, new Newtonsoft.Json.Converters.StringEnumConverter());
                var traceRecord = new PaymentTrace(
                    applicationId,
                    paymentId,
                    command.GatewayInternalTransactionId,
                    paymentNotificationJson,
                    command.IncomingRequestUri);

                await _paymentTracesTableProvider.Table.AddEntityAsync(traceRecord, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to record PaymentTrace, exception was: {Message}, stack trace: {Stack}", exception.Message, exception.StackTrace);
            }

            var getPaymentQuery = new GetPaymentDetails.Query
            {
                GatewayTypeId = gatewayTypeId,
                ApplicationId = applicationId,
                PaymentId = paymentId,
            };

            var getPaymentResult = await _mediator.Send(getPaymentQuery, cancellationToken);

            if (!getPaymentResult.IsSuccessful)
            {
                return Result.Failed(Result.FailureReason.UnableToLoadPayment, $"Unable to load payment with gatewayTypeId {gatewayTypeId}, applicationId {applicationId} and paymentId {paymentId}");
            }

            var payment = getPaymentResult.PaymentTableEntity;

            gateway.SetPaymentPropertiesFromNotification(payment, command.PaymentNotificationPayload);
            payment.SetStatus(paymentStatus);

            try
            {
                await _paymentsTableProvider.Table.UpdateEntityAsync(payment, payment.ETag, TableUpdateMode.Replace, cancellationToken);
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                _logger.LogCritical(
                    requestFailedException,
                    "Unable to update payments table for applicationId: {ApplicationId}, paymentId: {PaymentId}, due to ETag mismatch, exception message is: {Exception}",
                    applicationId, paymentId, requestFailedException.Message);

                return Result.Failed(
                    Result.FailureReason.ConflictUpdatingTableEntity,
                    $"Unable to update payments table for applicationId: {applicationId}, paymentId: {paymentId}, due to ETag mismatch, exception message is: {requestFailedException.Message}");
            }

            await PublishEvent(command, payment, cancellationToken);

            return Result.Success();
        }

        private async Task PublishEvent(
            Command command,
            IPaymentTableEntity payment,
            CancellationToken cancellationToken)
        {
            var eventData = new PaymentUpdatedEvent
            {
                CorrelationId = command.CorrelationId,
                ApplicationId = command.ApplicationId,
                PaymentId = command.PaymentId,
                Status = command.PaymentStatus,
                StatusChangedOn = payment.StatusChangedOn,
            };

            await _eventPublisher.PublishAsync(eventData, cancellationToken);
        }
    }
}