using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
using Firepuma.Payments.FunctionApp.Infrastructure.Exceptions;
using Firepuma.Payments.FunctionApp.PayFast.Config;
using Firepuma.Payments.FunctionApp.PayFast.Factories;
using Firepuma.Payments.FunctionApp.PayFast.TableModels;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.Implementations.TableStorage;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.PayFast.Commands;

public static class AddPayFastOnceOffPayment
{
    public class Command : BaseCommand, IRequest<Result>
    {
        [IgnoreCommandAudit]
        public string ApplicationSecret { get; set; }

        public ClientApplicationId ApplicationId { get; set; }
        public PaymentId PaymentId { get; set; }
        public string BuyerEmailAddress { get; set; }
        public string BuyerFirstName { get; set; }
        public double ImmediateAmountInRands { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public SplitPaymentConfig SplitPayment { get; set; }

        public class SplitPaymentConfig
        {
            public int MerchantId { get; set; }
            public int AmountInCents { get; set; }
            public int Percentage { get; set; }
            public int MinCents { get; set; }
            public int MaxCents { get; set; }
        }
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
            PaymentAlreadyExists,
            ApplicationSecretInvalid,
        }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IOptions<PayFastOptions> _payFastOptions;
        private readonly ILogger<Handler> _logger;
        private readonly PayFastClientAppConfigProvider _appConfigProvider;
        private readonly ITableProvider<IPaymentTableEntity> _onceOffPaymentsTableProvider;

        public Handler(
            IOptions<PayFastOptions> payFastOptions,
            ILogger<Handler> logger,
            PayFastClientAppConfigProvider appConfigProvider,
            ITableProvider<IPaymentTableEntity> onceOffPaymentsTableProvider)
        {
            _payFastOptions = payFastOptions;
            _logger = logger;
            _appConfigProvider = appConfigProvider;
            _onceOffPaymentsTableProvider = onceOffPaymentsTableProvider;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            try
            {
                var applicationConfig = await _appConfigProvider.GetApplicationConfigAsync(
                    command.ApplicationId,
                    command.ApplicationSecret,
                    cancellationToken);

                var validateAndStoreItnUrlWithAppName = AddApplicationIdToItnBaseUrl(_payFastOptions.Value.ValidateAndStoreItnBaseUrl, command.ApplicationId);

                var payment = new PayFastOnceOffPayment(
                    command.ApplicationId,
                    command.PaymentId,
                    command.BuyerEmailAddress,
                    command.BuyerFirstName,
                    command.ImmediateAmountInRands,
                    command.ItemName,
                    command.ItemDescription);

                var payFastSettings = PayFastSettingsFactory.CreatePayFastSettingsOld(
                    applicationConfig,
                    validateAndStoreItnUrlWithAppName,
                    payment.PaymentId,
                    command.ReturnUrl,
                    command.CancelUrl);

                var payfastRequest = PayFastRequestFactory.CreateOnceOffPaymentRequest(
                    payFastSettings,
                    new PaymentId(payment.PaymentId),
                    command.BuyerEmailAddress,
                    command.BuyerFirstName,
                    command.ImmediateAmountInRands,
                    command.ItemName,
                    command.ItemDescription);

                var redirectUrl = PayFastRedirectFactory.CreateRedirectUrl(
                    _logger,
                    payFastSettings,
                    payfastRequest,
                    command.SplitPayment);

                try
                {
                    await _onceOffPaymentsTableProvider.AddEntityAsync(payment, cancellationToken);
                }
                catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.Conflict)
                {
                    _logger.LogCritical(requestFailedException, "The payment (id '{CommandPaymentId}' and application id '{CommandApplicationId}') is already added and cannot be added again", command.PaymentId, command.ApplicationId);
                    return Result.Failed(Result.FailureReason.PaymentAlreadyExists, $"The payment (id '{command.PaymentId}' and application id '{command.ApplicationId}') is already added and cannot be added again");
                }

                return Result.Success(redirectUrl);
            }
            catch (ApplicationSecretInvalidException)
            {
                return Result.Failed(Result.FailureReason.ApplicationSecretInvalid, "Application secret is invalid");
            }
        }

        private static string AddApplicationIdToItnBaseUrl(string validateAndStoreItnBaseUrl, ClientApplicationId applicationId)
        {
            var questionMarkIndex = validateAndStoreItnBaseUrl.IndexOf("?", StringComparison.Ordinal);

            return questionMarkIndex >= 0
                ? validateAndStoreItnBaseUrl.Substring(0, questionMarkIndex).TrimEnd('/') + $"/{applicationId}?{validateAndStoreItnBaseUrl.Substring(questionMarkIndex + 1)}"
                : validateAndStoreItnBaseUrl + $"/{applicationId}";
        }
    }
}