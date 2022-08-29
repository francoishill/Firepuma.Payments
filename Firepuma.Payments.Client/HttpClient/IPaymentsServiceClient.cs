using Firepuma.Payments.Core.ClientDtos.ClientRequests;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;

// ReSharper disable UnusedMember.Global

namespace Firepuma.Payments.Client.HttpClient;

public interface IPaymentsServiceClient
{
    Task<PreparePayFastOnceOffPaymentResponse> PreparePayFastOnceOffPayment(
        PreparePayFastOnceOffPaymentRequest requestDTO,
        CancellationToken cancellationToken);
    
    Task<PayFastOnceOffPaymentResponse> GetPayFastPaymentTransactionDetails(
        string paymentId,
        CancellationToken cancellationToken);
}