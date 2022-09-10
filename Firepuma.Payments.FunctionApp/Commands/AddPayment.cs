﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.ClientDtos.ClientRequests.ExtraValues;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Attributes;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.Repositories;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.FunctionApp.Gateways;
using Firepuma.Payments.FunctionApp.Infrastructure.Config;
using FluentValidation;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

[assembly: InternalsVisibleTo("Firepuma.Payments.Tests")]

namespace Firepuma.Payments.FunctionApp.Commands;

public static class AddPayment
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public PaymentGatewayTypeId GatewayTypeId { get; init; }

        public ClientApplicationId ApplicationId { get; init; }

        [IgnoreCommandAudit]
        public PaymentApplicationConfig ApplicationConfig { get; init; }

        public PaymentId PaymentId { get; init; }
        public IPreparePaymentExtraValues ExtraValues { get; init; }
    }

    public class Result
    {
        public Uri RedirectUrl { get; init; }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.GatewayTypeId.Value).NotEmpty();
            RuleFor(x => x.ApplicationId.Value).NotEmpty();
            RuleFor(x => x.PaymentId.Value).NotEmpty();

            RuleFor(x => x.ApplicationConfig).NotNull();
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IOptions<PaymentGeneralOptions> _paymentOptions;
        private readonly ILogger<Handler> _logger;
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly IPaymentRepository _paymentRepository;

        public Handler(
            IOptions<PaymentGeneralOptions> paymentOptions,
            ILogger<Handler> logger,
            IEnumerable<IPaymentGateway> gateways,
            IPaymentRepository paymentRepository)
        {
            _paymentOptions = paymentOptions;
            _logger = logger;
            _gateways = gateways;
            _paymentRepository = paymentRepository;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var gatewayTypeId = command.GatewayTypeId;
            var applicationId = command.ApplicationId;
            var applicationConfig = command.ApplicationConfig;
            var paymentId = command.PaymentId;

            var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

            if (gateway == null)
            {
                throw new PreconditionFailedException($"The payment gateway type '{command.GatewayTypeId}' is not supported");
            }

            var paymentEntityExtraValues = await gateway.CreatePaymentEntityExtraValuesAsync(
                applicationId,
                paymentId,
                command.ExtraValues,
                cancellationToken);

            var paymentEntity = new PaymentEntity(
                applicationId,
                gatewayTypeId,
                paymentId,
                paymentEntityExtraValues);

            try
            {
                await _paymentRepository.AddItemAsync(paymentEntity, cancellationToken);
            }
            catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogCritical(
                    cosmosException,
                    "The payment (id \'{PaymentId}\' and application id \'{ApplicationId}\') is already added and cannot be added again",
                    paymentId, applicationId);

                throw new PreconditionFailedException($"The payment (id '{paymentId}' and application id '{applicationId}') is already added and cannot be added again");
            }

            var validateAndStorePaymentNotificationBaseUrlWithAppName = AddApplicationIdToPaymentNotificationBaseUrl(
                _paymentOptions.Value.ValidateAndStorePaymentNotificationBaseUrl,
                gatewayTypeId,
                applicationId);

            const string transactionIdQueryParamName = "tx";
            var backendNotifyUrl =
                validateAndStorePaymentNotificationBaseUrlWithAppName
                + (validateAndStorePaymentNotificationBaseUrlWithAppName.Contains('?') ? "&" : "?")
                + $"{transactionIdQueryParamName}={WebUtility.UrlEncode(paymentId.Value)}";

            var redirectUrl = await gateway.CreateRedirectUriAsync(
                applicationConfig,
                applicationId,
                paymentId,
                command.ExtraValues,
                backendNotifyUrl,
                cancellationToken);

            return new Result
            {
                RedirectUrl = redirectUrl,
            };
        }

        internal static string AddApplicationIdToPaymentNotificationBaseUrl(
            string validateAndStorePaymentNotificationBaseUrl,
            PaymentGatewayTypeId gatewayTypeId,
            ClientApplicationId applicationId)
        {
            var questionMarkIndex = validateAndStorePaymentNotificationBaseUrl.IndexOf("?", StringComparison.Ordinal);

            return questionMarkIndex >= 0
                ? validateAndStorePaymentNotificationBaseUrl.Substring(0, questionMarkIndex).TrimEnd('/') + $"/{gatewayTypeId}/{applicationId}?{validateAndStorePaymentNotificationBaseUrl.Substring(questionMarkIndex + 1)}"
                : validateAndStorePaymentNotificationBaseUrl + $"/{gatewayTypeId}/{applicationId}";
        }
    }
}