﻿using System.Net;
using Firepuma.Payments.FunctionApp.PayFast.ValueObjects;
using Firepuma.Payments.Implementations.Config;

namespace Firepuma.Payments.FunctionApp.PayFast.Factories;

public static class PayFastSettingsFactory
{
    public const string TRANSACTION_ID_QUERY_PARAM_NAME = "tx";

    public static PayFastPaymentSettings CreatePayFastSettings(
        PayFastClientAppConfig clientAppConfig,
        string backendNotifyUrl,
        string returnUrl,
        string cancelUrl)
    {
        return new PayFastPaymentSettings
        {
            MerchantId = clientAppConfig.MerchantId,
            MerchantKey = clientAppConfig.MerchantKey,
            PassPhrase = clientAppConfig.PassPhrase,
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,
            NotifyUrl = backendNotifyUrl,
            ProcessUrl = clientAppConfig.IsSandbox ? "https://sandbox.payfast.co.za/eng/process" : "https://www.payfast.co.za/eng/process",
            ValidateUrl = GetValidateUrl(clientAppConfig.IsSandbox),
        };
    }

    public static PayFastPaymentSettings CreatePayFastSettingsOld(
        PayFastClientAppConfig clientAppConfig,
        string backendNotifyUrl,
        string transactionId,
        string returnUrl,
        string cancelUrl)
    {
        var notifyUrl =
            backendNotifyUrl
            + (backendNotifyUrl.Contains('?') ? "&" : "?")
            + $"{TRANSACTION_ID_QUERY_PARAM_NAME}={WebUtility.UrlEncode(transactionId)}";

        return new PayFastPaymentSettings
        {
            MerchantId = clientAppConfig.MerchantId,
            MerchantKey = clientAppConfig.MerchantKey,
            PassPhrase = clientAppConfig.PassPhrase,
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,
            NotifyUrl = notifyUrl,
            ProcessUrl = clientAppConfig.IsSandbox ? "https://sandbox.payfast.co.za/eng/process" : "https://www.payfast.co.za/eng/process",
            ValidateUrl = GetValidateUrl(clientAppConfig.IsSandbox),
        };
    }

    public static string GetValidateUrl(bool isSandbox)
    {
        return isSandbox ? "https://sandbox.payfast.co.za/eng/query/validate" : "https://www.payfast.co.za/eng/query/validate";
    }
}