using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.Exceptions;
using Firepuma.PaymentsService.FunctionApp.PayFast.Config;
using Firepuma.PaymentsService.FunctionApp.PayFast.Factories;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableModels;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableProviders;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Commands;

public static class AddPayFastOnceOffPayment
{
    public class Command : BaseCommand, IRequest<Result>
    {
        [IgnoreCommandAudit]
        public string ApplicationSecret { get; set; }

        public string ApplicationId { get; set; }
        public string PaymentId { get; set; }
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
        private readonly PayFastOnceOffPaymentsTableProvider _onceOffPaymentsTableProvider;

        public Handler(
            IOptions<PayFastOptions> payFastOptions,
            ILogger<Handler> logger,
            PayFastClientAppConfigProvider appConfigProvider,
            PayFastOnceOffPaymentsTableProvider onceOffPaymentsTableProvider)
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

                var payFastSettings = PayFastSettingsFactory.CreatePayFastSettings(
                    applicationConfig,
                    validateAndStoreItnUrlWithAppName,
                    payment.PaymentId.Value,
                    command.ReturnUrl,
                    command.CancelUrl);

                var payfastRequest = PayFastRequestFactory.CreateOnceOffPaymentRequest(
                    payFastSettings,
                    payment.PaymentId,
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
                    await _onceOffPaymentsTableProvider.Table.ExecuteAsync(TableOperation.Insert(payment), cancellationToken);
                }
                catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                {
                    return Result.Failed(Result.FailureReason.PaymentAlreadyExists, $"The payment (id '{command.PaymentId}' and application id '{command.ApplicationId}') is already added and cannot be added again");
                }

                return Result.Success(redirectUrl);
            }
            catch (ApplicationSecretInvalidException)
            {
                return Result.Failed(Result.FailureReason.ApplicationSecretInvalid, "Application secret is invalid");
            }
        }

        private static string AddApplicationIdToItnBaseUrl(string validateAndStoreItnBaseUrl, string applicationId)
        {
            var questionMarkIndex = validateAndStoreItnBaseUrl.IndexOf("?", StringComparison.Ordinal);

            return questionMarkIndex >= 0
                ? validateAndStoreItnBaseUrl.Substring(0, questionMarkIndex).TrimEnd('/') + $"/{applicationId}?{validateAndStoreItnBaseUrl.Substring(questionMarkIndex + 1)}"
                : validateAndStoreItnBaseUrl + $"/{applicationId}";
        }
    }
}