﻿using Firepuma.PaymentsService.Abstractions.ValueObjects;
using Firepuma.PaymentsService.FunctionApp.PayFast.ValueObjects;
using PayFast;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Factories;

public static class PayFastRequestFactory
{
    public static PayFastRequest CreateOnceOffPaymentRequest(
        PayFastPaymentSettings payFastSettings,
        PayFastPaymentId paymentId,
        string buyerEmailAddress,
        string buyerFirstName,
        double immediateAmount,
        string itemName,
        string itemDescription)
    {
        var request = new PayFastRequest(payFastSettings.PassPhrase)
        {
            // Merchant Details
            merchant_id = payFastSettings.MerchantId,
            merchant_key = payFastSettings.MerchantKey,
            return_url = payFastSettings.ReturnUrl,
            cancel_url = payFastSettings.CancelUrl,
            notify_url = payFastSettings.NotifyUrl,

            // Buyer Details
            email_address = buyerEmailAddress,

            // Transaction Details
            m_payment_id = paymentId.Value,
            amount = immediateAmount,
            item_name = itemName,
            item_description = itemDescription,

            // Transaction Options
            email_confirmation = true,
            confirmation_address = buyerEmailAddress,
            name_first = buyerFirstName,
        };

        return request;
    }
}