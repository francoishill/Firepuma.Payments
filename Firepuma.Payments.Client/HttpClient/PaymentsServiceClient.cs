using System.ComponentModel.DataAnnotations;
using Firepuma.Payments.Client.Configuration;
using Firepuma.Payments.Core.ClientDtos.ClientRequests;
using Firepuma.Payments.Core.ClientDtos.ClientRequests.ExtraValues;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Firepuma.Payments.Client.HttpClient;

internal class PaymentsServiceClient : IPaymentsServiceClient
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly IOptions<PaymentServiceClientOptions> _options;
    private readonly ILogger<PaymentsServiceClient> _logger;

    public PaymentsServiceClient(
        ILogger<PaymentsServiceClient> logger,
        IOptions<PaymentServiceClientOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options;
        _httpClient = httpClientFactory.CreateClient(HttpClientConstants.PAYMENTS_SERVICE_HTTP_CLIENT_NAME);
    }

    public async Task<PreparePaymentResponse> PreparePayment(
        PaymentGatewayTypeId gatewayTypeId,
        PaymentId paymentId,
        IPreparePaymentExtraValues extraValues,
        CancellationToken cancellationToken)
    {
        if (!ValidationHelpers.ValidateDataAnnotations(extraValues, out var validationResultsForExtraValues))
        {
            throw new ValidationException(string.Join(". ", new[] { "ExtraValues is invalid" }.Concat(validationResultsForExtraValues.Select(s => s.ErrorMessage))));
        }

        var extraValuesCasted = PreparePaymentRequest.CastToExtraValues(extraValues);

        var prepareRequest = new PreparePaymentRequest
        {
            PaymentId = paymentId,
            ExtraValues = extraValuesCasted,
        };

        var postBody = new StringContent(JsonConvert.SerializeObject(prepareRequest, new Newtonsoft.Json.Converters.StringEnumConverter()));

        var applicationId = _options.Value.ApplicationId;

        var responseMessage = await _httpClient.PostAsync($"PreparePayment/{gatewayTypeId.Value}/{applicationId}", postBody, cancellationToken);

        if (!responseMessage.IsSuccessStatusCode)
        {
            var body = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Unable to prepare payment, status code was {Code}, body: {Body}", (int)responseMessage.StatusCode, body);
            responseMessage.EnsureSuccessStatusCode();
        }

        var rawBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            _logger.LogError("Body is empty, cannot PreparePayment");
            throw new InvalidOperationException("Body is empty, cannot PreparePayment");
        }

        var responseDTO = JsonConvert.DeserializeObject<PreparePaymentResponse>(rawBody);
        if (responseDTO == null)
        {
            _logger.LogError("Json parsed body is null when trying to deserialize body '{RawBody}' as PreparePaymentResponse", rawBody);
            throw new InvalidOperationException($"Json parsed body is null when trying to deserialize as PreparePaymentResponse");
        }

        return responseDTO;
    }

    public async Task<GetPaymentResponse> GetPaymentDetails(string paymentId, CancellationToken cancellationToken)
    {
        var applicationId = _options.Value.ApplicationId;
        var responseMessage = await _httpClient.GetAsync($"GetPayment/{applicationId}/{paymentId}", cancellationToken);

        if (!responseMessage.IsSuccessStatusCode)
        {
            var body = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Unable to get payment, status code was {Code}, body: {Body}", (int)responseMessage.StatusCode, body);
            responseMessage.EnsureSuccessStatusCode();
        }

        var rawBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            _logger.LogError("Body is empty, cannot GetPaymentDetails");
            throw new InvalidOperationException("Body is empty, cannot GetPaymentDetails");
        }

        var responseDTO = JsonConvert.DeserializeObject<GetPaymentResponse>(rawBody);
        if (responseDTO == null)
        {
            _logger.LogError("Json parsed body is null when trying to deserialize body '{RawBody}' as GetPaymentResponse", rawBody);
            throw new InvalidOperationException($"Json parsed body is null when trying to deserialize as GetPaymentResponse");
        }

        return responseDTO;
    }
}