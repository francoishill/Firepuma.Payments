using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Firepuma.Payments.Core.ClientDtos.ClientRequests;
using Firepuma.Payments.Core.Constants;
using Firepuma.Payments.Core.Infrastructure.Validation;
using Firepuma.Payments.Core.ValueObjects;
using Firepuma.Payments.FunctionApp.PayFast.Factories;
using Firepuma.Payments.FunctionApp.PayFast.TableModels;
using Firepuma.Payments.FunctionApp.PayFast.ValueObjects;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions.Results;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.Payments.TableModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PayFast;

namespace Firepuma.Payments.FunctionApp.PayFast;

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

    public async Task<ResultContainer<PrepareRequestResult, PrepareRequestFailureReason>> DeserializePrepareRequestAsync(HttpRequest req, CancellationToken cancellationToken)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var requestDTO = JsonConvert.DeserializeObject<PreparePayFastOnceOffPaymentRequest>(requestBody);

        if (requestDTO == null)
        {
            return ResultContainer<PrepareRequestResult, PrepareRequestFailureReason>.Failed(
                PrepareRequestFailureReason.ValidationFailed,
                "Request body is required but empty");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(requestDTO, out var validationResultsForRequest))
        {
            return ResultContainer<PrepareRequestResult, PrepareRequestFailureReason>.Failed(
                PrepareRequestFailureReason.ValidationFailed,
                new[] { "Request body is invalid" }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage)).ToArray());
        }

        if (requestDTO.SplitPayment != null)
        {
            if (!ValidationHelpers.ValidateDataAnnotations(requestDTO.SplitPayment, out var validationResultsForSplitPayment))
            {
                return ResultContainer<PrepareRequestResult, PrepareRequestFailureReason>.Failed(
                    PrepareRequestFailureReason.ValidationFailed,
                    new[] { "SplitPayment is invalid" }.Concat(validationResultsForSplitPayment.Select(s => s.ErrorMessage)).ToArray());
            }
        }

        var successfulValue = new PrepareRequestResult
        {
            PaymentId = requestDTO.PaymentId,
            RequestDto = requestDTO,
        };

        return ResultContainer<PrepareRequestResult, PrepareRequestFailureReason>.Success(successfulValue);
    }

    public async Task<Dictionary<string, object>> CreatePaymentEntityExtraValuesAsync(
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        CancellationToken cancellationToken)
    {
        if (genericRequestDto is not PreparePayFastOnceOffPaymentRequest requestDTO)
        {
            throw new NotSupportedException($"RequestDto is incorrect type in CreatePaymentTableEntity, it should be PreparePayFastOnceOffPaymentRequest but it is '{genericRequestDto.GetType().FullName}'");
        }

        var payment = new PayFastPaymentExtraValues(
            requestDTO.BuyerEmailAddress,
            requestDTO.BuyerFirstName,
            requestDTO.ImmediateAmountInRands ?? throw new ArgumentNullException(nameof(requestDTO.ImmediateAmountInRands)),
            requestDTO.ItemName,
            requestDTO.ItemDescription);

        await Task.CompletedTask;

        return PaymentEntity.CastToExtraValues(payment);
    }

    public async Task<Uri> CreateRedirectUriAsync(
        PaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        string backendNotifyUrl,
        CancellationToken cancellationToken)
    {
        if (genericRequestDto is not PreparePayFastOnceOffPaymentRequest requestDTO)
        {
            throw new NotSupportedException($"RequestDto is incorrect type in CreateRedirectUri, it should be PreparePayFastOnceOffPaymentRequest but it is '{genericRequestDto.GetType().FullName}'");
        }

        if (!genericApplicationConfig.TryCastExtraValuesToType<PayFastAppConfigExtraValues>(out var applicationConfig, out var castError))
        {
            throw new NotSupportedException($"Unable to cast ExtraValues to type PayFastClientAppConfig in CreateRedirectUriAsync, error: {castError}");
        }

        var payFastSettings = PayFastSettingsFactory.CreatePayFastSettings(
            applicationConfig,
            backendNotifyUrl,
            requestDTO.ReturnUrl,
            requestDTO.CancelUrl);

        var payfastRequest = PayFastRequestFactory.CreateOnceOffPaymentRequest(
            payFastSettings,
            paymentId,
            requestDTO.BuyerEmailAddress,
            requestDTO.BuyerFirstName,
            requestDTO.ImmediateAmountInRands ?? throw new ArgumentNullException(nameof(requestDTO.ImmediateAmountInRands)),
            requestDTO.ItemName,
            requestDTO.ItemDescription);

        var mappedCommandSplitPaymentConfig = _mapper.Map<PayFastRedirectFactory.SplitPaymentConfig>(requestDTO.SplitPayment);

        var redirectUrl = PayFastRedirectFactory.CreateRedirectUrl(
            _logger,
            payFastSettings,
            payfastRequest,
            mappedCommandSplitPaymentConfig);

        await Task.CompletedTask;
        return redirectUrl;
    }

    public async Task<ResultContainer<PaymentNotificationRequestResult, PaymentNotificationRequestFailureReason>> DeserializePaymentNotificationRequestAsync(
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
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogCritical("Request body is not form but contained content: {Body}", requestBody);

            return ResultContainer<PaymentNotificationRequestResult, PaymentNotificationRequestFailureReason>.Failed(
                PaymentNotificationRequestFailureReason.InvalidContentType,
                "Invalid content type, expected form data");
        }

        var payFastRequest = ExtractPayFastNotifyOrNull(req.Form);

        if (payFastRequest == null)
        {
            _logger.LogCritical("The body is null or empty, aborting processing of it");
            return ResultContainer<PaymentNotificationRequestResult, PaymentNotificationRequestFailureReason>.Failed(
                PaymentNotificationRequestFailureReason.RequestBodyIsNullOrEmpty,
                "The body is null or empty");
        }

        var successfulValue = new PaymentNotificationRequestResult
        {
            PaymentNotificationPayload = payFastRequest,
        };

        return ResultContainer<PaymentNotificationRequestResult, PaymentNotificationRequestFailureReason>.Success(successfulValue);
    }

    public async Task<ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>> ValidatePaymentNotificationAsync(
        PaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        object genericPaymentNotificationPayload,
        IPAddress remoteIp)
    {
        if (genericPaymentNotificationPayload is not PayFastNotificationPayload payFastNotificationPayload)
        {
            throw new NotSupportedException($"PaymentNotificationPayload is incorrect type in ValidatePaymentNotificationAsync, it should be PayFastNotificationPayload but it is '{genericPaymentNotificationPayload.GetType().FullName}'");
        }

        if (!genericApplicationConfig.TryCastExtraValuesToType<PayFastAppConfigExtraValues>(out var applicationConfig, out var castError))
        {
            throw new NotSupportedException($"Unable to cast ExtraValues to type PayFastClientAppConfig in ValidatePaymentNotificationAsync, error: {castError}");
        }

        var payFastRequest = payFastNotificationPayload.PayFastNotify;
        payFastRequest.SetPassPhrase(applicationConfig.PassPhrase);

        var calculatedSignature = payFastRequest.GetCalculatedSignature();
        var signatureIsValid = payFastRequest.signature == calculatedSignature;

        _logger.LogInformation("PayFast ITN signature valid: {IsValid}", signatureIsValid);
        if (!signatureIsValid)
        {
            _logger.LogCritical("PayFast ITN signature validation failed");
            return ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>.Failed(
                ValidatePaymentNotificationFailureReason.ValidationFailed,
                "PayFast ITN signature validation failed");
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
            return ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>.Failed(
                ValidatePaymentNotificationFailureReason.ValidationFailed,
                $"PayFast ITN merchant id validation failed, merchant id is {payFastRequest.merchant_id}");
        }

        var ipAddressValidationResult = await payfastValidator.ValidateSourceIp();
        _logger.LogInformation("Ip Address valid: {IpAddressValidationResult}, remote IP is {RemoteIp}", ipAddressValidationResult, remoteIp);
        if (!ipAddressValidationResult)
        {
            _logger.LogCritical("PayFast ITN IPAddress validation failed, ip is {RemoteIp}", remoteIp);
            return ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>.Failed(
                ValidatePaymentNotificationFailureReason.ValidationFailed,
                $"PayFast ITN IPAddress validation failed, ip is {remoteIp}");
        }

        // TODO: Currently seems that the data validation only works for success
        if (payFastRequest.payment_status == PayFastStatics.CompletePaymentConfirmation)
        {
            var dataValidationResult = await payfastValidator.ValidateData();
            _logger.LogInformation("Data Validation Result: {DataValidationResult}", dataValidationResult);
            if (!dataValidationResult)
            {
                _logger.LogCritical("PayFast ITN data validation failed");
                return ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>.Failed(
                    ValidatePaymentNotificationFailureReason.ValidationFailed,
                    "PayFast ITN data validation failed");
            }
        }

        if (payFastRequest.payment_status != PayFastStatics.CompletePaymentConfirmation
            && payFastRequest.payment_status != PayFastStatics.CancelledPaymentConfirmation)
        {
            _logger.LogCritical("Invalid PayFast ITN payment status '{Status}'", payFastRequest.payment_status);
            return ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>.Failed(
                ValidatePaymentNotificationFailureReason.ValidationFailed,
                $"Invalid PayFast ITN payment status '{payFastRequest.payment_status}'");
        }

        var paymentStatus = ConvertPayFastStatusToPaymentStatusOrNull(payFastRequest.payment_status);
        if (paymentStatus == null)
        {
            _logger.LogCritical("PayFast status is invalid and cannot convert PayFast status string \'{PaymentStatus}\' to PaymentStatus enum", payFastRequest.payment_status);
            return ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>.Failed(
                ValidatePaymentNotificationFailureReason.InvalidStatus,
                $"PayFast status is invalid and cannot convert PayFast status string '{payFastRequest.payment_status}' to PaymentStatus enum");
        }

        var successfulValue = new ValidatePaymentNotificationResult(
            new PaymentId(payFastRequest.m_payment_id),
            payFastRequest.pf_payment_id,
            paymentStatus.Value);

        return ResultContainer<ValidatePaymentNotificationResult, ValidatePaymentNotificationFailureReason>.Success(successfulValue);
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
                _logger.LogWarning("PayFast ITN for paymentId '{PaymentId}' does not have a payfastToken", genericPayment.PaymentId);
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

    private static PayFastNotificationPayload ExtractPayFastNotifyOrNull(IFormCollection formCollection)
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