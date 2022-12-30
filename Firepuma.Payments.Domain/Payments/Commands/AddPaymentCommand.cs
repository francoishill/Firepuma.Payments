using System.Net;
using System.Runtime.CompilerServices;
using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities.Attributes;
using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Abstractions.ExtraValues;
using Firepuma.Payments.Domain.Payments.Config;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.Extensions;
using Firepuma.Payments.Domain.Payments.Repositories;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

[assembly: InternalsVisibleTo("Firepuma.Payments.Tests")]

namespace Firepuma.Payments.Domain.Payments.Commands;

public static class AddPaymentCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required ClientApplicationId ApplicationId { get; init; }
        public required PaymentGatewayTypeId GatewayTypeId { get; init; }

        [IgnoreCommandExecution]
        public required PaymentApplicationConfig ApplicationConfig { get; init; } = null!;

        public required PaymentId PaymentId { get; init; }
        public required IPreparePaymentExtraValues ExtraValues { get; init; } = null!;
    }

    public class Result
    {
        public Uri RedirectUrl { get; init; } = null!;
    }

    public sealed class Validator : AbstractValidator<Payload>
    {
        public Validator()
        {
            RuleFor(x => x.GatewayTypeId.Value).NotEmpty();
            RuleFor(x => x.ApplicationId.Value).NotEmpty();
            RuleFor(x => x.PaymentId.Value).NotEmpty();

            RuleFor(x => x.ApplicationConfig).NotNull();
        }
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly IOptions<PaymentGeneralOptions> _paymentOptions;
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly IPaymentRepository _paymentRepository;

        public Handler(
            IOptions<PaymentGeneralOptions> paymentOptions,
            IEnumerable<IPaymentGateway> gateways,
            IPaymentRepository paymentRepository)
        {
            _paymentOptions = paymentOptions;
            _gateways = gateways;
            _paymentRepository = paymentRepository;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            var gatewayTypeId = payload.GatewayTypeId;
            var applicationId = payload.ApplicationId;
            var applicationConfig = payload.ApplicationConfig;
            var paymentId = payload.PaymentId;

            var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

            if (gateway == null)
            {
                throw new PreconditionFailedException($"The payment gateway type '{payload.GatewayTypeId}' is not supported");
            }

            var paymentEntityExtraValues = await gateway.CreatePaymentEntityExtraValuesAsync(
                applicationId,
                paymentId,
                payload.ExtraValues,
                cancellationToken);

            var paymentEntity = new PaymentEntity(
                applicationId,
                gatewayTypeId,
                paymentId,
                paymentEntityExtraValues);

            //TODO: Is the throwing of the commented out PreconditionFailedException below necessary
            await _paymentRepository.AddItemAsync(paymentEntity, cancellationToken);

            // try
            // {
            //     await _paymentRepository.AddItemAsync(paymentEntity, cancellationToken);
            // }
            // catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.Conflict)
            // {
            //     _logger.LogCritical(
            //         cosmosException,
            //         "The payment (id \'{PaymentId}\' and application id \'{ApplicationId}\') is already added and cannot be added again",
            //         paymentId, applicationId);
            //
            //     throw new PreconditionFailedException($"The payment (id '{paymentId}' and application id '{applicationId}') is already added and cannot be added again");
            // }

            var incomingPaymentNotificationWebhookBaseUrlWithAppName = AddApplicationIdToPaymentNotificationBaseUrl(
                _paymentOptions.Value.IncomingPaymentNotificationWebhookBaseUrl,
                applicationId,
                gatewayTypeId);

            const string transactionIdQueryParamName = "tx";
            var backendNotifyUrl =
                incomingPaymentNotificationWebhookBaseUrlWithAppName
                + (incomingPaymentNotificationWebhookBaseUrlWithAppName.Contains('?') ? "&" : "?")
                + $"{transactionIdQueryParamName}={WebUtility.UrlEncode(paymentId.Value)}";

            var redirectUrl = await gateway.CreateRedirectUriAsync(
                applicationConfig,
                applicationId,
                paymentId,
                payload.ExtraValues,
                backendNotifyUrl,
                cancellationToken);

            return new Result
            {
                RedirectUrl = redirectUrl,
            };
        }

        internal static string AddApplicationIdToPaymentNotificationBaseUrl(
            string incomingPaymentNotificationWebhookBaseUrlWithAppName,
            ClientApplicationId applicationId,
            PaymentGatewayTypeId gatewayTypeId)
        {
            var questionMarkIndex = incomingPaymentNotificationWebhookBaseUrlWithAppName.IndexOf("?", StringComparison.Ordinal);

            return questionMarkIndex >= 0
                ? incomingPaymentNotificationWebhookBaseUrlWithAppName.Substring(0, questionMarkIndex).TrimEnd('/') + $"/{applicationId}/{gatewayTypeId}?{incomingPaymentNotificationWebhookBaseUrlWithAppName.Substring(questionMarkIndex + 1)}"
                : incomingPaymentNotificationWebhookBaseUrlWithAppName + $"/{applicationId}/{gatewayTypeId}";
        }
    }
}