using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.BusMessages;
using Firepuma.Payments.FunctionApp.Infrastructure.MessageBus.Services;
using Firepuma.Payments.FunctionApp.PayFast.Config;
using Firepuma.Payments.FunctionApp.PayFast.Factories;
using MediatR;
using Microsoft.Extensions.Logging;
using PayFast;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.PayFast.Commands;

public static class EnqueuePayFastItnForProcessing
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public string CorrelationId { get; set; }
        public ClientApplicationId ApplicationId { get; set; }
        public PayFastNotify PayFastRequest { get; set; }
        public string RemoteIp { get; set; }
        public string IncomingRequestUri { get; set; }
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
            ValidationFailed,
        }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly PayFastClientAppConfigProvider _appConfigProvider;
        private readonly IPaymentsMessageSender _paymentsMessageSender;


        public Handler(
            ILogger<Handler> logger,
            PayFastClientAppConfigProvider appConfigProvider,
            IPaymentsMessageSender paymentsMessageSender)
        {
            _logger = logger;
            _appConfigProvider = appConfigProvider;
            _paymentsMessageSender = paymentsMessageSender;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var applicationId = command.ApplicationId;
            var payFastRequest = command.PayFastRequest;
            var remoteIp = IPAddress.Parse(command.RemoteIp);

            var applicationConfig = await _appConfigProvider.GetApplicationConfigAndSkipSecretCheckAsync(
                applicationId,
                cancellationToken);

            payFastRequest.SetPassPhrase(applicationConfig.PassPhrase);

            var calculatedSignature = payFastRequest.GetCalculatedSignature();
            var signatureIsValid = payFastRequest.signature == calculatedSignature;

            _logger.LogInformation("PayFast ITN signature valid: {IsValid}", signatureIsValid);
            if (!signatureIsValid)
            {
                _logger.LogCritical("PayFast ITN signature validation failed");
                return Result.Failed(Result.FailureReason.ValidationFailed, "PayFast ITN signature validation failed");
            }

            var subsetOfPayFastSettings = new PayFastSettings
            {
                MerchantId = applicationConfig.MerchantId,
                MerchantKey = applicationConfig.MerchantKey,
                PassPhrase = applicationConfig.PassPhrase,
                ValidateUrl = PayFastSettingsFactory.GetValidateUrl(applicationConfig.IsSandbox),
            };
            var payfastValidator = new PayFastValidator(subsetOfPayFastSettings, payFastRequest, remoteIp);

            var merchantIdValidationResult = payfastValidator.ValidateMerchantId();
            _logger.LogInformation(
                "Merchant Id valid result: {MerchantIdValidationResult}, merchant id is {RequestMerchantId}",
                merchantIdValidationResult, payFastRequest.merchant_id);

            if (!merchantIdValidationResult)
            {
                _logger.LogCritical("PayFast ITN merchant id validation failed, merchant id is {MerchantId}", payFastRequest.merchant_id);
                return Result.Failed(Result.FailureReason.ValidationFailed, $"PayFast ITN merchant id validation failed, merchant id is {payFastRequest.merchant_id}");
            }

            var ipAddressValidationResult = await payfastValidator.ValidateSourceIp();
            _logger.LogInformation("Ip Address valid: {IpAddressValidationResult}, remote IP is {RemoteIp}", ipAddressValidationResult, remoteIp);
            if (!ipAddressValidationResult)
            {
                _logger.LogCritical("PayFast ITN IPAddress validation failed, ip is {RemoteIp}", remoteIp);
                return Result.Failed(Result.FailureReason.ValidationFailed, $"PayFast ITN IPAddress validation failed, ip is {remoteIp}");
            }

            // TODO: Currently seems that the data validation only works for success
            if (payFastRequest.payment_status == PayFastStatics.CompletePaymentConfirmation)
            {
                var dataValidationResult = await payfastValidator.ValidateData();
                _logger.LogInformation("Data Validation Result: {DataValidationResult}", dataValidationResult);
                if (!dataValidationResult)
                {
                    _logger.LogCritical("PayFast ITN data validation failed");
                    return Result.Failed(Result.FailureReason.ValidationFailed, "PayFast ITN data validation failed");
                }
            }

            if (payFastRequest.payment_status != PayFastStatics.CompletePaymentConfirmation
                && payFastRequest.payment_status != PayFastStatics.CancelledPaymentConfirmation)
            {
                _logger.LogCritical("Invalid PayFast ITN payment status '{Status}'", payFastRequest.payment_status);
                return Result.Failed(Result.FailureReason.ValidationFailed, $"Invalid PayFast ITN payment status '{payFastRequest.payment_status}'");
            }

            var messageDto = new PayFastPaymentItnValidatedMessage(
                applicationId,
                new PaymentId(payFastRequest.m_payment_id),
                payFastRequest,
                command.IncomingRequestUri);

            await _paymentsMessageSender.SendAsync(
                messageDto,
                command.CorrelationId,
                cancellationToken);

            return Result.Success();
        }
    }
}