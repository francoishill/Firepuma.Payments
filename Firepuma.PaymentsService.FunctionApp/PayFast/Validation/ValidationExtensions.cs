using System.Collections.Generic;
using System.Linq;
using System.Net;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Validation;
using Firepuma.PaymentsService.Implementations.Config;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Validation;

public static class ValidationExtensions
{
    public static bool ValidateClientAppConfig(
        this PayFastClientAppConfig clientAppConfig,
        string applicationId,
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

        statusCode = HttpStatusCode.OK;
        errors = null;
        return true;
    }
}