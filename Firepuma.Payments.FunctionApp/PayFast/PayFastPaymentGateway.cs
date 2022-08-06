using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Firepuma.Payments.Abstractions.DTOs.Requests;
using Firepuma.Payments.Abstractions.Infrastructure.Validation;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage.Helpers;
using Firepuma.Payments.FunctionApp.PayFast.Commands;
using Firepuma.Payments.FunctionApp.PayFast.Config;
using Firepuma.Payments.FunctionApp.PayFast.Factories;
using Firepuma.Payments.FunctionApp.PayFast.TableModels;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions.Results;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.FunctionApp.TableProviders;
using Firepuma.Payments.Implementations.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Firepuma.Payments.FunctionApp.PayFast;

public class PayFastPaymentGateway : IPaymentGateway
{
    public PaymentGatewayTypeId TypeId => new("PayFast");
    public string DisplayName => "PayFast";

    public PaymentGatewayFeatures Features => new()
    {
        PreparePayment = true,
    };

    private readonly IOptions<PayFastOptions> _payFastOptions;
    private readonly ILogger<PayFastPaymentGateway> _logger;
    private readonly IMapper _mapper;

    public PayFastPaymentGateway(
        IOptions<PayFastOptions> payFastOptions,
        ILogger<PayFastPaymentGateway> logger,
        IMapper mapper)
    {
        _payFastOptions = payFastOptions;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IPaymentApplicationConfig> GetApplicationConfigAsync(
        ApplicationConfigsTableProvider applicationConfigsTableProvider,
        ClientApplicationId applicationId,
        CancellationToken cancellationToken)
    {
        var applicationConfig = await AzureTableHelper.GetSingleRecordOrNullAsync<PayFastClientAppConfig>(
            applicationConfigsTableProvider.Table,
            "PayFast",
            applicationId.Value,
            cancellationToken);

        return applicationConfig;
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

    public async Task<IPaymentTableEntity> CreatePaymentTableEntity(
        IPaymentApplicationConfig applicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        CancellationToken cancellationToken)
    {
        if (genericRequestDto is not PreparePayFastOnceOffPaymentRequest requestDTO)
        {
            throw new NotSupportedException($"RequestDto is incorrect type in CreatePaymentTableEntity, it should be PreparePayFastOnceOffPaymentRequest but it is '{genericRequestDto.GetType().FullName}'");
        }

        var payment = new PayFastOnceOffPayment(
            applicationId,
            paymentId,
            requestDTO.BuyerEmailAddress,
            requestDTO.BuyerFirstName,
            requestDTO.ImmediateAmountInRands ?? throw new ArgumentNullException(nameof(requestDTO.ImmediateAmountInRands)),
            requestDTO.ItemName,
            requestDTO.ItemDescription);

        await Task.CompletedTask;
        return payment;
    }

    public async Task<Uri> CreateRedirectUri(
        IPaymentApplicationConfig genericApplicationConfig,
        ClientApplicationId applicationId,
        PaymentId paymentId,
        object genericRequestDto,
        CancellationToken cancellationToken)
    {
        if (genericRequestDto is not PreparePayFastOnceOffPaymentRequest requestDTO)
        {
            throw new NotSupportedException($"RequestDto is incorrect type in CreateRedirectUri, it should be PreparePayFastOnceOffPaymentRequest but it is '{genericRequestDto.GetType().FullName}'");
        }

        if (genericApplicationConfig is not PayFastClientAppConfig applicationConfig)
        {
            throw new NotSupportedException($"ApplicationConfig is incorrect type in CreateRedirectUri, it should be PayFastClientAppConfig but it is '{genericApplicationConfig.GetType().FullName}'");
        }

        var validateAndStoreItnUrlWithAppName = AddApplicationIdToItnBaseUrl(
            _payFastOptions.Value.ValidateAndStoreItnBaseUrl,
            applicationId);

        var payFastSettings = PayFastSettingsFactory.CreatePayFastSettings(
            applicationConfig,
            validateAndStoreItnUrlWithAppName,
            paymentId.Value,
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

        var mappedCommandSplitPaymentConfig = _mapper.Map<AddPayFastOnceOffPayment.Command.SplitPaymentConfig>(requestDTO.SplitPayment);

        var redirectUrl = PayFastRedirectFactory.CreateRedirectUrl(
            _logger,
            payFastSettings,
            payfastRequest,
            mappedCommandSplitPaymentConfig);

        await Task.CompletedTask;
        return redirectUrl;
    }

    private static string AddApplicationIdToItnBaseUrl(string validateAndStoreItnBaseUrl, ClientApplicationId applicationId)
    {
        var questionMarkIndex = validateAndStoreItnBaseUrl.IndexOf("?", StringComparison.Ordinal);

        return questionMarkIndex >= 0
            ? validateAndStoreItnBaseUrl.Substring(0, questionMarkIndex).TrimEnd('/') + $"/{applicationId}?{validateAndStoreItnBaseUrl.Substring(questionMarkIndex + 1)}"
            : validateAndStoreItnBaseUrl + $"/{applicationId}";
    }
}