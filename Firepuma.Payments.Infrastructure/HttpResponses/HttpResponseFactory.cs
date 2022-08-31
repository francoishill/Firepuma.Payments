using Microsoft.AspNetCore.Mvc;

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
}