using System.Collections.Generic;
using System.Linq;
using System.Net;
using Firepuma.PaymentsService.Abstractions.Constants;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Validation;
using Firepuma.PaymentsService.Implementations.Config;
using Microsoft.AspNetCore.Http;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Validation;

public static class ValidationExtensions
{
    public static bool ValidateClientAppConfig(
        this ClientAppConfig clientAppConfig,
        string applicationId,
        IHeaderDictionary requestHeaders,
        out HttpStatusCode statusCode,
        out IEnumerable<string> errors)
    {
        if (clientAppConfig == null)
        {
            statusCode = HttpStatusCode.BadRequest;
            errors = new[] { $"Config not found for application with id {applicationId}" };
            return false;
        }

        if (!ValidationHelpers.ValidateDataAnnotations(clientAppConfig, out var validationResultsForConfig))
        {
            statusCode = HttpStatusCode.BadRequest;
            errors = new[] { "Application config is invalid" }.Concat(validationResultsForConfig.Select(s => s.ErrorMessage));
            return false;
        }

        var requestAppSecret = requestHeaders[PaymentHttpRequestHeaderKeys.APP_SECRET].FirstOrDefault();
        if (clientAppConfig.ApplicationSecret != requestAppSecret)
        {
            statusCode = HttpStatusCode.Unauthorized;
            errors = new[] { "Invalid or missing app secret" };
            return false;
        }

        statusCode = HttpStatusCode.OK;
        errors = null;
        return true;
    }
}