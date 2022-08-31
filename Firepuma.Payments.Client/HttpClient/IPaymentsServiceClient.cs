using Firepuma.Payments.Core.ClientDtos.ClientRequests.ExtraValues;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;
using Firepuma.Payments.Core.Payments.ValueObjects;

// ReSharper disable UnusedMember.Global

namespace Firepuma.Payments.Client.HttpClient;

public interface IPaymentsServiceClient
{
    Task<PreparePaymentResponse> PreparePayment(
        PaymentGatewayTypeId gatewayTypeId,
        PaymentId paymentId,
        IPreparePaymentExtraValues extraValues,
        CancellationToken cancellationToken);
    
    Task<GetPaymentResponse> GetPaymentDetails(
        string paymentId,
        CancellationToken cancellationToken);
}