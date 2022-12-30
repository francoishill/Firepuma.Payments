using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Firepuma.Payments.Domain.Payments.Abstractions;
using Firepuma.Payments.Domain.Payments.Config;
using Firepuma.Payments.Domain.Payments.Services.ServiceResults;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using Firepuma.Payments.Domain.Plumbing.Validation;
using Firepuma.Payments.Infrastructure.Gateways.PayFast.Config;

// ReSharper disable ArgumentsStyleNamedExpression

namespace Firepuma.Payments.Infrastructure.Gateways.PayFast;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class PayFastPaymentGatewayManager : IPaymentGatewayManager
{
    public PaymentGatewayTypeId TypeId => PaymentGatewayIds.PayFast;
    public string DisplayName => "PayFast";

    public async Task<CreateClientApplicationRequestResult> DeserializeCreateClientApplicationRequestAsync(
        JsonDocument requestBody,
        CancellationToken cancellationToken)
    {
        var requestDTO = requestBody.Deserialize<CreateRequest>();

        if (requestDTO == null)
        {
            throw new Exception("Request body is required but empty");
        }

        if (!ValidationHelpers.ValidateDataAnnotations(requestDTO, out var validationResultsForRequest))
        {
            throw new Exception(string.Join(". ", new[] { "Request body is invalid" }.Concat(validationResultsForRequest.Select(s => s.ErrorMessage ?? "[NULL error]")).ToArray()));
        }

        var successfulValue = new CreateClientApplicationRequestResult
        {
            RequestDto = requestDTO,
        };

        await Task.CompletedTask;
        return successfulValue;
    }

    public Dictionary<string, string> CreatePaymentApplicationConfigExtraValues(object genericRequestDto)
    {
        if (genericRequestDto is not CreateRequest requestDTO)
        {
            throw new NotSupportedException($"RequestDto is incorrect type in CreatePaymentApplicationConfig, it should be CreatePayFastClientApplicationRequest but it is '{genericRequestDto.GetType().FullName}'");
        }

        var extraValues = PayFastAppConfigExtraValues.CreateExtraValuesDictionary(
            isSandbox: requestDTO.IsSandbox,
            merchantId: requestDTO.MerchantId,
            merchantKey: requestDTO.MerchantKey,
            passPhrase: requestDTO.PassPhrase);

        return extraValues;
    }

    private class CreateRequest
    {
        public bool IsSandbox { get; set; }

        [Required]
        public string MerchantId { get; set; } = null!;

        [Required]
        public string MerchantKey { get; set; } = null!;

        public string PassPhrase { get; set; } = null!;
    }
}