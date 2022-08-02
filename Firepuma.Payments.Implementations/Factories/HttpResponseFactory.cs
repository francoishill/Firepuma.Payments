using Microsoft.AspNetCore.Mvc;

namespace Firepuma.Payments.Implementations.Factories;

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