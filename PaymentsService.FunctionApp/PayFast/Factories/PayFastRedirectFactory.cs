using System;
using System.Collections.Generic;
using System.Net;
using Firepuma.PaymentsService.FunctionApp.PayFast.Commands;
using Firepuma.PaymentsService.FunctionApp.PayFast.ValueObjects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PayFast;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Factories;

public static class PayFastRedirectFactory
{
    public static Uri CreateRedirectUrl(
        ILogger logger,
        PayFastPaymentSettings payFastSettings,
        PayFastRequest payfastRequest,
        AddPayFastOnceOffPayment.Command.SplitPaymentConfig splitPaymentConfig)
    {
        var fullQuery = payfastRequest.ToString();

        if (splitPaymentConfig != null)
        {
            var splitPaymentSetupJsonString = GetSplitPaymentSetupJsonString(
                logger,
                splitPaymentConfig);

            fullQuery += $"&setup={WebUtility.UrlEncode(splitPaymentSetupJsonString)}";
        }

        var redirectUriBuilder = new UriBuilder(payFastSettings.ProcessUrl)
        {
            Query = fullQuery
        };

        return redirectUriBuilder.Uri;
    }

    private static string GetSplitPaymentSetupJsonString(
        ILogger logger,
        AddPayFastOnceOffPayment.Command.SplitPaymentConfig splitPaymentConfig)
    {
        try
        {
            var splitPaymentValues = new Dictionary<string, object>
            {
                { "merchant_id", splitPaymentConfig.MerchantId },
            };

            if (splitPaymentConfig.AmountInCents > 0) splitPaymentValues.Add("amount", splitPaymentConfig.AmountInCents);
            if (splitPaymentConfig.Percentage > 0) splitPaymentValues.Add("percentage", splitPaymentConfig.Percentage);
            if (splitPaymentConfig.MinCents > 0) splitPaymentValues.Add("min", splitPaymentConfig.MinCents);
            if (splitPaymentConfig.MaxCents > 0) splitPaymentValues.Add("max", splitPaymentConfig.MaxCents);

            var splitPaymentData = new Dictionary<string, object>
            {
                {
                    "split_payment", splitPaymentValues
                },
            };

            var splitPaymentJson = JsonConvert.SerializeObject(splitPaymentData, new Newtonsoft.Json.Converters.StringEnumConverter());
            return splitPaymentJson;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to serialize split_payment data for Payfast payment, error: {Error}", exception.Message);
            throw new Exception($"Unable to serialize split_payment data for Payfast payment, error: {exception.Message}");
        }
    }
}