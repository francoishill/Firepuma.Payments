using Firepuma.CommandsAndQueries.Abstractions.Exceptions;
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

    public static IActionResult CreateResponseMessageResult(this CommandException commandException)
    {
        return new JsonResult(new
        {
            Errors = commandException.Errors,
        })
        {
            StatusCode = (int)commandException.StatusCode,
        };
    }
}