using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
using Firepuma.DatabaseRepositories.Abstractions.Exceptions;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Entities.Extensions;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.Queries;
using Firepuma.Payments.Domain.Payments.Repositories;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.Domain.Payments.Commands;

public static class UpdatePaymentCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required ClientApplicationId ApplicationId { get; init; }
        public required PaymentGatewayTypeId GatewayTypeId { get; init; }

        public required PaymentId PaymentId { get; init; }
        public required string GatewayInternalTransactionId { get; init; } = null!;

        public required PaymentStatus PaymentStatus { get; init; }

        public required BasePaymentNotificationPayload PaymentNotificationPayload { get; init; } = null!;

        public required string IncomingRequestUri { get; init; } = null!;
        public required string CorrelationId { get; init; } = null!;
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
            RuleFor(x => x.PaymentId.Value).NotEmpty();
            RuleFor(x => x.IncomingRequestUri).NotEmpty();

            RuleFor(x => x.PaymentNotificationPayload).NotNull();
        }
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly IPaymentNotificationTraceRepository _paymentTracesRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMediator _mediator;

        public Handler(
            ILogger<Handler> logger,
            IEnumerable<IPaymentGateway> gateways,
            IPaymentNotificationTraceRepository paymentTracesRepository,
            IPaymentRepository paymentRepository,
            IMediator mediator)
        {
            _logger = logger;
            _gateways = gateways;
            _paymentTracesRepository = paymentTracesRepository;
            _paymentRepository = paymentRepository;
            _mediator = mediator;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            var gatewayTypeId = payload.GatewayTypeId;
            var applicationId = payload.ApplicationId;
            var paymentId = payload.PaymentId;
            var paymentStatus = payload.PaymentStatus;

            _logger.LogInformation(
                "Updating payment Id {PaymentId} (gateway {GatewayTypeId}, applicationId {ApplicationId}) with status \'{S}\'",
                paymentId, gatewayTypeId, applicationId, paymentStatus.ToString());

            var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

            if (gateway == null)
            {
                throw new PreconditionFailedException($"The payment gateway type '{payload.GatewayTypeId}' is not supported");
            }

            try
            {
                var paymentNotificationJson = JsonConvert.SerializeObject(payload.PaymentNotificationPayload, new Newtonsoft.Json.Converters.StringEnumConverter());
                var traceRecord = new PaymentNotificationTrace(
                    applicationId,
                    gatewayTypeId,
                    paymentId,
                    payload.GatewayInternalTransactionId,
                    paymentNotificationJson,
                    payload.IncomingRequestUri);

                await _paymentTracesRepository.AddItemAsync(traceRecord, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to record PaymentTrace, exception was: {Message}, stack trace: {Stack}", exception.Message, exception.StackTrace);
            }

            var getPaymentQuery = new GetPaymentDetailsQuery.Payload
            {
                ApplicationId = applicationId,
                PaymentId = paymentId,
            };

            var getPaymentResult = await _mediator.Send(getPaymentQuery, cancellationToken);

            var payment = getPaymentResult.PaymentEntity;

            if (payment == null)
            {
                _logger.LogCritical(
                    "Unable to load payment for applicationId: {ApplicationId} and paymentId: {PaymentId}, it was null",
                    applicationId, paymentId);

                throw new Exception($"Unable to load payment for applicationId: {applicationId} and paymentId: {paymentId}, it was null");
            }

            gateway.SetPaymentPropertiesFromNotification(payment, payload.PaymentNotificationPayload);
            payment.SetStatus(paymentStatus);

            try
            {
                await _paymentRepository.UpsertItemAsync(payment, cancellationToken: cancellationToken);
            }
            catch (DocumentETagMismatchException documentETagMismatchException)
            {
                _logger.LogCritical(
                    documentETagMismatchException,
                    "Unable to update payment for applicationId: {ApplicationId}, paymentId: {PaymentId}, due to ETag mismatch",
                    applicationId, paymentId);

                throw new PreconditionFailedException(
                    $"Unable to update payment for applicationId: {applicationId}, paymentId: {paymentId}, due to ETag mismatch, exception message is: {documentETagMismatchException.Message}");
            }

            //TODO: Ensure the Result above inherits from BaseIntegrationEventProducingCommandResponse to cause integration events
            // await PublishEvent(payload, payment, cancellationToken);

            return new Result();
        }

        // private async Task PublishEvent(
        //     Payload payload,
        //     PaymentEntity payment,
        //     CancellationToken cancellationToken)
        // {
        //     var eventData = new PaymentUpdatedEvent
        //     {
        //         CorrelationId = payload.CorrelationId,
        //         ApplicationId = payload.ApplicationId,
        //         PaymentId = payload.PaymentId,
        //         Status = payload.PaymentStatus,
        //         StatusChangedOn = payment.StatusChangedOn,
        //     };
        //
        //     await _eventPublisher.PublishAsync(eventData, cancellationToken);
        // }
    }
}