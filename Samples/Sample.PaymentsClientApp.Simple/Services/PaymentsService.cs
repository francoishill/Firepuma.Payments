using Firepuma.Payments.Client.HttpClient;
using Firepuma.Payments.Core.ClientDtos.ClientRequests.ExtraValues;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;
using Firepuma.Payments.Core.Constants;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.Core.Results.ValueObjects;

namespace Sample.PaymentsClientApp.Simple.Services;

public class PaymentsService
{
    private readonly ILogger<PaymentsService> _logger;
    private readonly IPaymentsServiceClient _paymentsServiceClient;

    public PaymentsService(
        ILogger<PaymentsService> logger,
        IPaymentsServiceClient paymentsServiceClient)
    {
        _logger = logger;
        _paymentsServiceClient = paymentsServiceClient;
    }

    public async Task<ResultContainer<GetAvailablePaymentGatewaysResponse[], GetAvailablePaymentGatewaysFailureReasons>> GetAvailablePaymentGateways(
        CancellationToken cancellationToken)
    {
        return await _paymentsServiceClient.GetAvailablePaymentGateways(cancellationToken);
    }

    public async Task<ResultContainer<PreparePaymentResponse, PreparePaymentFailureReason>> PreparePayfastOnceOffPayment(
        PaymentId newPaymentId,
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

        var extraValues = new PreparePayFastOnceOffPaymentExtraValues
        {
            BuyerEmailAddress = buyerEmail,
            BuyerFirstName = buyerName,

            ImmediateAmountInRands = immediateAmount,

            ItemName = itemName,
            ItemDescription = itemDescription,

            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,

            //TODO: remove SplitPayment or use it to split payment between main merchant and another merchant
            // SplitPayment = new PreparePayFastOnceOffPaymentExtraValues.SplitPaymentConfig
            // {
            //     MerchantId = anotherMerchantId,
            //     AmountInCents = amountInRandsToPayForAnotherMerchant * 100,
            // },
        };

        var preparedPaymentResult = await _paymentsServiceClient.PreparePayment(
            PaymentGatewayIds.PayFast,
            newPaymentId,
            extraValues,
            cancellationToken);

        return preparedPaymentResult;
    }

    public async Task<ResultContainer<GetPaymentResponse, GetPaymentFailureReason>> GetPaymentDetails(string paymentId, CancellationToken cancellationToken)
    {
        var payment = await _paymentsServiceClient.GetPaymentDetails(paymentId, cancellationToken);

        return payment;
    }
}