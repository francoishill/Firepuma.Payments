using Firepuma.PaymentsService.Abstractions.DTOs.Requests;
using Firepuma.PaymentsService.Abstractions.DTOs.Responses;

// ReSharper disable UnusedMember.Global

namespace PaymentsService.Client.HttpClient;

public interface IPaymentsServiceClient
{
    Task<PreparePayFastOnceOffPaymentResponse> PreparePayFastOnceOffPayment(
        PreparePayFastOnceOffPaymentRequest requestDTO,
        CancellationToken cancellationToken);
    
    Task<PayFastOnceOffPaymentResponse> GetPayFastPaymentTransactionDetails(
        string paymentId,
        CancellationToken cancellationToken);
}