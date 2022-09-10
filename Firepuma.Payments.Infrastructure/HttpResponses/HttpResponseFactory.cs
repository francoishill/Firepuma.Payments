using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Exceptions;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Firepuma.Payments.Infrastructure.HttpResponses;

public static class HttpResponseFactory
{
    public static IActionResult CreateBadRequestResponse(params string[] errors)
    {
        return new BadRequestObjectResult(new Dictionary<string, object>
        {
            { "Errors", errors }
        });
    }

    public static IActionResult CreateResponseMessageResult(this WrappedRequestException wrappedRequestException)
    {
        return new JsonResult(new
        {
            Errors = wrappedRequestException.Errors,
        })
        {
            StatusCode = (int)wrappedRequestException.StatusCode,
        };
    }
}