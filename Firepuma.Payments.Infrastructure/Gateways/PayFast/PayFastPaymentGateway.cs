using System.Net;
using AutoMapper;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Abstractions.ExtraValues;
using Firepuma.Payments.Domain.Payments.Abstractions.Results;
using Firepuma.Payments.Domain.Payments.Config;
using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using Firepuma.Payments.Domain.Plumbing.Validation;
using Firepuma.Payments.Infrastructure.Gateways.PayFast.Config;
using Firepuma.Payments.Infrastructure.Gateways.PayFast.Factories;
using Firepuma.Payments.Infrastructure.Gateways.PayFast.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PayFast;

#pragma warning disable CS8618

namespace Firepuma.Payments.Infrastructure.Gateways.PayFast;

public class PayFastPaymentGateway : IPaymentGateway
{
    public PaymentGatewayTypeId TypeId => PaymentGatewayIds.PayFast;
    public string DisplayName => "PayFast";

    public PaymentGatewayFeatures Features => new()
    {
        PreparePayment = true,
    };

    private readonly ILogger<PayFastPaymentGateway> _logger;
    private readonly IMapper _mapper;

    // ReSharper disable once UnusedMember.Global
    public PayFastPaymentGateway()
    {
        // used in unit tests (like PaymentGatewaysTests)
    }

    public PayFastPaymentGateway(
        ILogger<PayFastPaymentGateway> logger,
        IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ValidatePrepareRequestResult> ValidatePrepareRequestAsync(
        PreparePaymentRequest preparePaymentRequest,
        CancellationToken cancellationToken)
    {
        if (!preparePaymentRequest.TryCastExtraValuesToType<PreparePayFastOnceOffPaymentExtraValues>(out var extraValues, out var castError))
        {
            throw new Exception($"The ExtraValues of PreparePaymentRequest should be type PreparePayFastOnceOffPaymentExtraValues, error: {castError}");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(extraValues, out var validationResultsForRequest))
        {
            throw new Exception(string.Join(". ", new[] { "ExtraValues is invalid" }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage ?? "[NULL error]")).ToArray()));
        }

        if (extraValues.SplitPayment != null)
        {
            if (!ValidationHelpers.ValidateDataAnnotations(extraValues.SplitPayment, out var validationResultsForSplitPayment))
            {
                throw new Exception(string.Join(". ", new[] { "ExtraValues is invalid" }.Concat(validationResultsForSplitPayment.Select(s => s.ErrorMessage ?? "[NULL error]")).ToArray()));
            }
        }

        await Task.CompletedTask;

        var successResult = new ValidatePrepareRequestResult
        {
            ExtraValues = extraValues,
        };

        return successResult;
    }

    public async Task<string> CreatePaymentEntityExtraValuesAsync(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        IPreparePaymentExtraValues genericExtraValues,
        CancellationToken cancellationToken)
    {
        if (genericExtraValues is not PreparePayFastOnceOffPaymentExtraValues extraValues)
        {
            throw new NotSupportedException($"ExtraValues is incorrect type in CreatePaymentEntityExtraValuesAsync, it should be PreparePayFastOnceOffPaymentExtraValues but it is '{genericExtraValues.GetType().FullName}'");
        }

        var payment = new PayFastPaymentExtraValues
        {
            EmailAddress = extraValues.BuyerEmailAddress,
            NameFirst = extraValues.BuyerFirstName,
            ImmediateAmountInRands = extraValues.ImmediateAmountInRands ?? throw new ArgumentNullException(nameof(extraValues.ImmediateAmountInRands)),
            ItemName = extraValues.ItemName,
            ItemDescription = extraValues.ItemDescription,
        };

        await Task.CompletedTask;

        return PaymentEntity.CastToExtraValues(payment);
    }

    public async Task<Uri> CreateRedirectUriAsync(
        PaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        IPreparePaymentExtraValues genericExtraValues,
        string backendNotifyUrl,
        CancellationToken cancellationToken)
    {
        if (genericExtraValues is not PreparePayFastOnceOffPaymentExtraValues extraValues)
        {
            throw new NotSupportedException($"ExtraValues is incorrect type in CreateRedirectUriAsync, it should be PreparePayFastOnceOffPaymentExtraValues but it is '{genericExtraValues.GetType().FullName}'");
        }

        var applicationConfig = PayFastAppConfigExtraValues.CreateFromExtraValues(genericApplicationConfig.ExtraValues);

        var payFastSettings = PayFastSettingsFactory.CreatePayFastSettings(
            applicationConfig,
            backendNotifyUrl,
            extraValues.ReturnUrl,
            extraValues.CancelUrl);

        var payfastRequest = PayFastRequestFactory.CreateOnceOffPaymentRequest(
            payFastSettings,
            paymentId,
            extraValues.BuyerEmailAddress,
            extraValues.BuyerFirstName,
            extraValues.ImmediateAmountInRands ?? throw new ArgumentNullException(nameof(extraValues.ImmediateAmountInRands)),
            extraValues.ItemName,
            extraValues.ItemDescription);

        var mappedCommandSplitPaymentConfig = _mapper.Map<PayFastRedirectFactory.SplitPaymentConfig>(extraValues.SplitPayment);

        var redirectUrl = PayFastRedirectFactory.CreateRedirectUrl(
            _logger,
            payFastSettings,
            payfastRequest,
            mappedCommandSplitPaymentConfig);

        await Task.CompletedTask;
        return redirectUrl;
    }

    public async Task<PaymentNotificationRequestResult> DeserializePaymentNotificationRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken)
    {
        var transactionIdQueryParam = req.Query[PayFastSettingsFactory.TRANSACTION_ID_QUERY_PARAM_NAME];
        if (!string.IsNullOrWhiteSpace(transactionIdQueryParam))
        {
            _logger.LogInformation("Found the {ParamName} query param from URL with value {TransactionId}", PayFastSettingsFactory.TRANSACTION_ID_QUERY_PARAM_NAME, transactionIdQueryParam);
        }

        if (!req.HasFormContentType)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            _logger.LogCritical("Request body is not form but contained content: {Body}", requestBody);

            throw new Exception("Invalid content type, expected form data");
        }

        var payFastRequest = ExtractPayFastNotifyOrNull(req.Form);

        if (payFastRequest == null)
        {
            _logger.LogCritical("The body is null or empty, aborting processing of it");
            throw new Exception("The body is null or empty");
        }

        var successfulValue = new PaymentNotificationRequestResult
        {
            PaymentNotificationPayload = payFastRequest,
        };

        return successfulValue;
    }

    public async Task<ValidatePaymentNotificationResult> ValidatePaymentNotificationAsync(
        PaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        object genericPaymentNotificationPayload,
        IPAddress remoteIp)
    {
        if (genericPaymentNotificationPayload is not PayFastNotificationPayload payFastNotificationPayload)
        {
            throw new NotSupportedException($"PaymentNotificationPayload is incorrect type in ValidatePaymentNotificationAsync, it should be PayFastNotificationPayload but it is '{genericPaymentNotificationPayload.GetType().FullName}'");
        }

        var applicationConfig = PayFastAppConfigExtraValues.CreateFromExtraValues(genericApplicationConfig.ExtraValues);

        var payFastRequest = payFastNotificationPayload.PayFastNotify;
        payFastRequest.SetPassPhrase(applicationConfig.PassPhrase);

        var calculatedSignature = payFastRequest.GetCalculatedSignature();
        var signatureIsValid = payFastRequest.signature == calculatedSignature;

        _logger.LogInformation("PayFast ITN signature valid: {IsValid}", signatureIsValid);
        if (!signatureIsValid)
        {
            _logger.LogCritical("PayFast ITN signature validation failed");
            throw new Exception("PayFast ITN signature validation failed");
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
            throw new Exception($"PayFast ITN merchant id validation failed, merchant id is {payFastRequest.merchant_id}");
        }

        var ipAddressValidationResult = await payfastValidator.ValidateSourceIp();
        _logger.LogInformation("Ip Address valid: {IpAddressValidationResult}, remote IP is {RemoteIp}", ipAddressValidationResult, remoteIp);
        if (!ipAddressValidationResult)
        {
            _logger.LogCritical("PayFast ITN IPAddress validation failed, ip is {RemoteIp}", remoteIp);
            throw new Exception($"PayFast ITN IPAddress validation failed, ip is {remoteIp}");
        }

        // TODO: Currently seems that the data validation only works for success
        if (payFastRequest.payment_status == PayFastStatics.CompletePaymentConfirmation)
        {
            var dataValidationResult = await payfastValidator.ValidateData();
            _logger.LogInformation("Data Validation Result: {DataValidationResult}", dataValidationResult);
            if (!dataValidationResult)
            {
                _logger.LogCritical("PayFast ITN data validation failed");
                throw new Exception("PayFast ITN data validation failed");
            }
        }

        if (payFastRequest.payment_status != PayFastStatics.CompletePaymentConfirmation
            && payFastRequest.payment_status != PayFastStatics.CancelledPaymentConfirmation)
        {
            _logger.LogCritical("Invalid PayFast ITN payment status '{Status}'", payFastRequest.payment_status);
            throw new Exception($"Invalid PayFast ITN payment status '{payFastRequest.payment_status}'");
        }

        var paymentStatus = ConvertPayFastStatusToPaymentStatusOrNull(payFastRequest.payment_status);
        if (paymentStatus == null)
        {
            _logger.LogCritical("PayFast status is invalid and cannot convert PayFast status string \'{PaymentStatus}\' to PaymentStatus enum", payFastRequest.payment_status);
            throw new Exception($"PayFast status is invalid and cannot convert PayFast status string '{payFastRequest.payment_status}' to PaymentStatus enum");
        }

        var successfulValue = new ValidatePaymentNotificationResult(
            new PaymentId(payFastRequest.m_payment_id),
            payFastRequest.pf_payment_id,
            paymentStatus.Value);

        return successfulValue;
    }

    public void SetPaymentPropertiesFromNotification(PaymentEntity genericPayment, BasePaymentNotificationPayload genericPaymentNotificationPayload)
    {
        if (!genericPayment.TryCastExtraValuesToType<PayFastPaymentExtraValues>(out var payFastPayment, out var castError))
        {
            throw new NotSupportedException($"Unable to cast ExtraValues to type PayFastOnceOffPayment in SetPaymentPropertiesFromNotification, error: {castError}");
        }

        if (genericPaymentNotificationPayload is not PayFastNotificationPayload payFastNotificationPayload)
        {
            throw new NotSupportedException($"PaymentNotificationPayload is incorrect type in ValidatePaymentNotificationAsync, it should be PayFastNotify but it is '{genericPaymentNotificationPayload.GetType().FullName}'");
        }

        var payFastRequest = payFastNotificationPayload.PayFastNotify;
        if (payFastRequest.payment_status == PayFastStatics.CompletePaymentConfirmation)
        {
            if (string.IsNullOrWhiteSpace(payFastRequest.token))
            {
                _logger.LogInformation("PayFast ITN for paymentId '{PaymentId}' does not have a payfastToken", genericPayment.PaymentId);
            }

            payFastPayment.PayfastPaymentToken = payFastRequest.token;
        }
    }

    private static PaymentStatus? ConvertPayFastStatusToPaymentStatusOrNull(string payFastStatus)
    {
        return payFastStatus switch
        {
            PayFastStatics.CompletePaymentConfirmation => PaymentStatus.Succeeded,
            PayFastStatics.CancelledPaymentConfirmation => PaymentStatus.Cancelled,
            _ => null,
        };
    }

    private static PayFastNotificationPayload? ExtractPayFastNotifyOrNull(IFormCollection? formCollection)
    {
        // https://github.com/louislewis2/payfast/blob/master/src/PayFast.AspNetCore/PayFastNotifyModelBinder.cs
        if (formCollection == null || formCollection.Count < 1)
        {
            return null;
        }

        var properties = new Dictionary<string, string>();

        foreach (var key in formCollection.Keys)
        {
            formCollection.TryGetValue(key, value: out var value);

            properties.Add(key, value);
        }

        var model = new PayFastNotify();
        model.FromFormCollection(properties);

        return new PayFastNotificationPayload
        {
            PayFastNotify = model,
        };
    }
}