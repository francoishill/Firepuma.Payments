using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Config;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.TableStorage;
using MediatR;
using Microsoft.Extensions.Options;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.Commands;

public static class AddPayment
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public PaymentGatewayTypeId GatewayTypeId { get; init; }

        public ClientApplicationId ApplicationId { get; init; }

        [IgnoreCommandAudit]
        public IPaymentApplicationConfig ApplicationConfig { get; init; }

        public PaymentId PaymentId { get; init; }
        public object RequestDto { get; init; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public Uri RedirectUrl { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            Uri redirectUrl,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;

            RedirectUrl = redirectUrl;

            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success(Uri redirectUrl)
        {
            return new Result(true, redirectUrl, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, null, reason, errors);
        }

        public enum FailureReason
        {
            UnknownGatewayTypeId,
            PaymentAlreadyExists,
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IOptions<PaymentGeneralOptions> _paymentOptions;
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly ITableService<IPaymentTableEntity> _paymentsTableService;

        public Handler(
            IOptions<PaymentGeneralOptions> paymentOptions,
            IEnumerable<IPaymentGateway> gateways,
            ITableService<IPaymentTableEntity> paymentsTableService)
        {
            _paymentOptions = paymentOptions;
            _gateways = gateways;
            _paymentsTableService = paymentsTableService;
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
                return Result.Failed(Result.FailureReason.UnknownGatewayTypeId, $"The payment gateway type '{command.GatewayTypeId}' is not supported");
            }

            var paymentEntity = await gateway.CreatePaymentTableEntityAsync(
                applicationConfig,
                applicationId,
                paymentId,
                command.RequestDto,
                cancellationToken);

            paymentEntity.GatewayTypeId = gateway.TypeId.Value;

            try
            {
                await _paymentsTableService.AddEntityAsync(paymentEntity, cancellationToken);
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
            {
                return Result.Failed(Result.FailureReason.PaymentAlreadyExists, $"The payment (id '{paymentId}' and application id '{applicationId}') is already added and cannot be added again");
            }

            var validateAndStorePaymentNotificationBaseUrlWithAppName = AddApplicationIdToItnBaseUrl(
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
                command.RequestDto,
                backendNotifyUrl,
                cancellationToken);

            return Result.Success(redirectUrl);
        }

        private static string AddApplicationIdToItnBaseUrl(
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