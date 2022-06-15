using System.Net;
using Firepuma.PaymentsService.FunctionApp.PayFast.Config;
using Firepuma.PaymentsService.FunctionApp.PayFast.ValueObjects;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Factories;

public static class PayFastSettingsFactory
{
    public const string TRANSACTION_ID_QUERY_PARAM_NAME = "tx";

    public static PayFastPaymentSettings CreatePayFastSettings(
        ApplicationConfig applicationConfig,
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
            MerchantId = applicationConfig.MerchantId,
            MerchantKey = applicationConfig.MerchantKey,
            PassPhrase = applicationConfig.PassPhrase,
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,
            NotifyUrl = notifyUrl,
            ProcessUrl = applicationConfig.IsSandbox ? "https://sandbox.payfast.co.za/eng/process" : "https://www.payfast.co.za/eng/process",
            ValidateUrl = GetValidateUrl(applicationConfig.IsSandbox),
        };
    }

    public static string GetValidateUrl(bool isSandbox)
    {
        return isSandbox ? "https://sandbox.payfast.co.za/eng/query/validate" : "https://www.payfast.co.za/eng/query/validate";
    }
}