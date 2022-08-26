using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.DTOs.Requests;
using Firepuma.Payments.Abstractions.Infrastructure.Validation;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionAppManager.GatewayAbstractions;
using Firepuma.Payments.FunctionAppManager.GatewayAbstractions.Results;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.TableStorage;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Firepuma.Payments.FunctionAppManager.PayFast;

public class PayFastPaymentGatewayManager : IPaymentGatewayManager
{
    public PaymentGatewayTypeId TypeId => new("PayFast");
    public string DisplayName => "PayFast";

    private readonly ITableService<BasePaymentApplicationConfig> _applicationConfigsTableService;

    public PayFastPaymentGatewayManager(
        ITableService<BasePaymentApplicationConfig> applicationConfigsTableService)
    {
        _applicationConfigsTableService = applicationConfigsTableService;
    }

    public async Task<ResultContainer<CreateClientApplicationRequestResult, CreateClientApplicationRequestFailureReason>> DeserializeCreateClientApplicationRequestAsync(
        HttpRequest req,
        CancellationToken cancellationToken)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var requestDTO = JsonConvert.DeserializeObject<CreatePayFastClientApplicationRequest>(requestBody);

        if (requestDTO == null)
        {
            return ResultContainer<CreateClientApplicationRequestResult, CreateClientApplicationRequestFailureReason>.Failed(
                CreateClientApplicationRequestFailureReason.ValidationFailed,
                "Request body is required but empty");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(requestDTO, out var validationResultsForRequest))
        {
            return ResultContainer<CreateClientApplicationRequestResult, CreateClientApplicationRequestFailureReason>.Failed(
                CreateClientApplicationRequestFailureReason.ValidationFailed,
                new[] { "Request body is invalid" }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage)).ToArray());
        }

        var successfulValue = new CreateClientApplicationRequestResult
        {
            RequestDto = requestDTO,
        };

        return ResultContainer<CreateClientApplicationRequestResult, CreateClientApplicationRequestFailureReason>.Success(successfulValue);
    }

    public BasePaymentApplicationConfig CreatePaymentApplicationConfig(
        ClientApplicationId applicationId,
        object genericRequestDto,
        string applicationSecret)
    {
        if (genericRequestDto is not CreatePayFastClientApplicationRequest requestDTO)
        {
            throw new NotSupportedException($"RequestDto is incorrect type in CreatePaymentApplicationConfig, it should be CreatePayFastClientApplicationRequest but it is '{genericRequestDto.GetType().FullName}'");
        }

        var newClientAppConfig = new PayFastClientAppConfig(
            applicationId,
            applicationSecret,
            requestDTO.IsSandbox,
            requestDTO.MerchantId,
            requestDTO.MerchantKey,
            requestDTO.PassPhrase);

        return newClientAppConfig;
    }

    public async Task<BasePaymentApplicationConfig> GetApplicationConfigAsync(
        ClientApplicationId applicationId,
        CancellationToken cancellationToken)
    {
        var applicationConfig = await _applicationConfigsTableService.GetEntityAsync<PayFastClientAppConfig>(
            "PayFast",
            applicationId.Value,
            cancellationToken: cancellationToken);

        return applicationConfig;
    }
}