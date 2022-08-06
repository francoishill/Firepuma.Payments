﻿using Firepuma.Payments.Abstractions.ValueObjects;

namespace Firepuma.Payments.Abstractions.DTOs.Responses;

public class PreparePayFastOnceOffPaymentResponse
{
    public PaymentId PaymentId { get; set; }
    public string RedirectUrl { get; set; }

    public PreparePayFastOnceOffPaymentResponse(PaymentId paymentId, Uri redirectUrl)
    {
        PaymentId = paymentId;
        RedirectUrl = redirectUrl.AbsoluteUri;
    }
}