using AutoMapper;
using Firepuma.Payments.Abstractions.DTOs.Requests;
using Firepuma.Payments.Client.HttpClient;
using Sample.PaymentsClientApp.Simple.Services.Results;

namespace Sample.PaymentsClientApp.Simple.Services;

public class PaymentsService
{
    private readonly ILogger<PaymentsService> _logger;
    private readonly IPaymentsServiceClient _paymentsServiceClient;
    private readonly IMapper _mapper;

    public PaymentsService(
        ILogger<PaymentsService> logger,
        IPaymentsServiceClient paymentsServiceClient,
        IMapper mapper)
    {
        _logger = logger;
        _paymentsServiceClient = paymentsServiceClient;
        _mapper = mapper;
    }

    public async Task<PreparePayfastOnceOffPaymentResult> PreparePayfastOnceOffPayment(
        string newPaymentId,
        string returnUrl,
        string cancelUrl,
        CancellationToken cancellationToken)
    {
        //TODO: add code to create Payment in database
        _logger.LogError("TODO: add code to create Payment in database");

        //TODO: replace constants with logic to get buyer email
        const string buyerEmail = "sample-buyer@email.com";
        const string buyerName = "Sample Buyer";
        const double immediateAmount = 123.56;
        const string itemName = "Purchased item name";
        const string itemDescription = "Purchased item description";

        var requestDTO = new PreparePayFastOnceOffPaymentRequest
        {
            PaymentId = newPaymentId, // $"{(long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds}-{Guid.NewGuid().ToString()}",

            BuyerEmailAddress = buyerEmail,
            BuyerFirstName = buyerName,

            ImmediateAmountInRands = immediateAmount,

            ItemName = itemName,
            ItemDescription = itemDescription,

            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,

            //TODO: remove SplitPayment or use it to split payment between main merchant and another merchant
            // SplitPayment = new PreparePayFastOnceOffPaymentRequest.SplitPaymentConfig
            // {
            //     MerchantId = anotherMerchantId,
            //     AmountInCents = amountInRandsToPayForAnotherMerchant * 100,
            // },
        };

        var preparedPayment = await _paymentsServiceClient.PreparePayFastOnceOffPayment(requestDTO, cancellationToken);
        var redirectUri = new Uri(preparedPayment.RedirectUrl);

        return new PreparePayfastOnceOffPaymentResult(redirectUri, preparedPayment.PaymentId);
    }

    public async Task<PayfastOnceOffPaymentResult> GetPayfastOnceOffPayment(string paymentId, CancellationToken cancellationToken)
    {
        var payment = await _paymentsServiceClient.GetPayFastPaymentTransactionDetails(paymentId, cancellationToken);

        var mappedPayment = _mapper.Map<PayfastOnceOffPaymentResult>(payment);

        return mappedPayment;
    }
}