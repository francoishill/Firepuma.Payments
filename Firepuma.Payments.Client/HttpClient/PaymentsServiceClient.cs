using System.Net;
using Firepuma.Payments.Client.Configuration;
using Firepuma.Payments.Core.ClientDtos.ClientRequests;
using Firepuma.Payments.Core.ClientDtos.ClientRequests.ExtraValues;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Results.ValueObjects;
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

    public async Task<ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>> PreparePayment(
        PaymentGatewayTypeId gatewayTypeId,
        PaymentId paymentId,
        IPreparePaymentExtraValues extraValues,
        CancellationToken cancellationToken)
    {
        if (!ValidationHelpers.ValidateDataAnnotations(extraValues, out var validationResultsForExtraValues))
        {
            return ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>.Failed(
                PreparePaymentFailureReason.ValidationFailed,
                new[] { "ExtraValues is invalid" }.Concat(validationResultsForExtraValues.Select(s => s.ErrorMessage)).ToArray());
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
            _logger.LogError("Prepare payment failed, status code was {Code}, body: {Body}", (int)responseMessage.StatusCode, body);

            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                return ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>.Failed(
                    PreparePaymentFailureReason.BadRequestResponse,
                    $"Prepare payment failed with BadRequest, body: {body}");
            }

            return ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>.Failed(
                PreparePaymentFailureReason.UnexpectedFailure,
                $"Prepare payment failed with status code {responseMessage.StatusCode.ToString()}");
        }

        var rawBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            _logger.LogError("Body is empty, cannot PreparePayment");
            return ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>.Failed(
                PreparePaymentFailureReason.UnableToDeserializeBody,
                $"Prepare payment failed, content body is empty although it was successful (status code was {responseMessage.StatusCode.ToString()})");
        }

        var responseDTO = JsonConvert.DeserializeObject<PreparePaymentResponse>(rawBody);
        if (responseDTO == null)
        {
            _logger.LogError("Json parsed body is null when trying to deserialize body '{RawBody}' as PreparePaymentResponse", rawBody);
            return ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>.Failed(
                PreparePaymentFailureReason.UnableToDeserializeBody,
                $"Json parsed body is null when trying to deserialize as PreparePaymentResponse");
        }

        return ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>.Success(responseDTO);
    }

    public async Task<ResultContainer<GetPaymentResponse, GetPaymentFailureReason>> GetPaymentDetails(string paymentId, CancellationToken cancellationToken)
    {
        var applicationId = _options.Value.ApplicationId;
        var responseMessage = await _httpClient.GetAsync($"GetPayment/{applicationId}/{paymentId}", cancellationToken);

        if (!responseMessage.IsSuccessStatusCode)
        {
            var body = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Unable to get payment, status code was {Code}, body: {Body}", (int)responseMessage.StatusCode, body);

            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                return ResultContainer<GetPaymentResponse, GetPaymentFailureReason>.Failed(
                    GetPaymentFailureReason.BadRequestResponse,
                    $"Get payment failed with BadRequest, body: {body}");
            }

            return ResultContainer<GetPaymentResponse, GetPaymentFailureReason>.Failed(
                GetPaymentFailureReason.UnexpectedFailure,
                $"Get payment failed with status code {responseMessage.StatusCode.ToString()}");
        }

        var rawBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            _logger.LogError("Body is empty, cannot GetPaymentDetails");

            return ResultContainer<GetPaymentResponse, GetPaymentFailureReason>.Failed(
                GetPaymentFailureReason.UnableToDeserializeBody,
                $"Get payment failed, content body is empty although it was successful (status code was {responseMessage.StatusCode.ToString()})");
        }

        var responseDTO = JsonConvert.DeserializeObject<GetPaymentResponse>(rawBody);
        if (responseDTO == null)
        {
            _logger.LogError("Json parsed body is null when trying to deserialize body '{RawBody}' as GetPaymentResponse", rawBody);

            return ResultContainer<GetPaymentResponse, GetPaymentFailureReason>.Failed(
                GetPaymentFailureReason.UnableToDeserializeBody,
                $"Json parsed body is null when trying to deserialize as GetPaymentResponse");
        }

        return ResultContainer<GetPaymentResponse, GetPaymentFailureReason>.Success(responseDTO);
    }
}