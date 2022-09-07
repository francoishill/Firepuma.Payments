using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Constants;
using Firepuma.Payments.Core.Infrastructure.Validation;
using Firepuma.Payments.Core.PaymentAppConfiguration.Entities;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Results.ValueObjects;
using Firepuma.Payments.FunctionAppManager.Gateways.PayFast.Requests;
using Firepuma.Payments.FunctionAppManager.Gateways.Results;
using Firepuma.Payments.Infrastructure.Gateways.PayFast;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Firepuma.Payments.FunctionAppManager.Gateways.PayFast;

public class PayFastPaymentGatewayManager : IPaymentGatewayManager
{
    public PaymentGatewayTypeId TypeId => PaymentGatewayIds.PayFast;
    public string DisplayName => "PayFast";

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

    public JObject CreatePaymentApplicationConfigExtraValues(object genericRequestDto)
    {
        if (genericRequestDto is not CreatePayFastClientApplicationRequest requestDTO)
        {
            throw new NotSupportedException($"RequestDto is incorrect type in CreatePaymentApplicationConfig, it should be CreatePayFastClientApplicationRequest but it is '{genericRequestDto.GetType().FullName}'");
        }

        var newClientAppConfig = new PayFastAppConfigExtraValues(
            requestDTO.IsSandbox,
            requestDTO.MerchantId,
            requestDTO.MerchantKey,
            requestDTO.PassPhrase);

        return PaymentApplicationConfig.CastToExtraValues(newClientAppConfig);
    }
}