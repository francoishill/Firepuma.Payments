using Firepuma.Payments.Core.DTOs.Requests;
using Firepuma.Payments.Core.DTOs.Responses;

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