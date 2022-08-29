﻿using System.ComponentModel.DataAnnotations;
using Firepuma.Payments.Client.Configuration;
using Firepuma.Payments.Core.ClientDtos.ClientRequests;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;
using Firepuma.Payments.Core.Constants;
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

    public async Task<PreparePayFastOnceOffPaymentResponse> PreparePayFastOnceOffPayment(
        PreparePayFastOnceOffPaymentRequest requestDTO,
        CancellationToken cancellationToken)
    {
        if (!ValidationHelpers.ValidateDataAnnotations(requestDTO, out var validationResultsForDTO))
        {
            throw new ValidationException(string.Join(". ", new[] { "Request DTO is invalid" }.Concat(validationResultsForDTO.Select(s => s.ErrorMessage))));
        }

        var postBody = new StringContent(JsonConvert.SerializeObject(requestDTO, new Newtonsoft.Json.Converters.StringEnumConverter()));

        var applicationId = _options.Value.ApplicationId;

        var responseMessage = await _httpClient.PostAsync($"PreparePayment/{PaymentGatewayIds.PayFast.Value}/{applicationId}", postBody, cancellationToken);

        if (!responseMessage.IsSuccessStatusCode)
        {
            var body = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Unable to prepare PayFast once-off payment, status code was {Code}, body: {Body}", (int)responseMessage.StatusCode, body);
            responseMessage.EnsureSuccessStatusCode();
        }

        var rawBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            _logger.LogError("Body is empty, cannot PreparePayFastOnceOffPayment");
            throw new InvalidOperationException("Body is empty, cannot PreparePayFastOnceOffPayment");
        }

        var responseDTO = JsonConvert.DeserializeObject<PreparePayFastOnceOffPaymentResponse>(rawBody);
        if (responseDTO == null)
        {
            _logger.LogError("Json parsed body is null when trying to deserialize body '{RawBody}' as PreparePayFastOnceOffPaymentResponse", rawBody);
            throw new InvalidOperationException($"Json parsed body is null when trying to deserialize as PreparePayFastOnceOffPaymentResponse");
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