using Firepuma.Payments.Core.ClientDtos.ClientRequests.ExtraValues;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Results.ValueObjects;

// ReSharper disable UnusedMember.Global

namespace Firepuma.Payments.Client.HttpClient;

public interface IPaymentsServiceClient
{
    Task<ResultContainer<GetAvailablePaymentGatewaysResponse[], GetAvailablePaymentGatewaysFailureReasons>> GetAvailablePaymentGatewaysAsync(
        CancellationToken cancellationToken
    );

    Task<ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>> PreparePaymentAsync(
        PaymentGatewayTypeId gatewayTypeId,
        PaymentId paymentId,
        IPreparePaymentExtraValues extraValues,
        CancellationToken cancellationToken);

    Task<ResultContainer<GetPaymentResponse, GetPaymentFailureReason>> GetPaymentDetailsAsync(
        string paymentId,
        CancellationToken cancellationToken);
}