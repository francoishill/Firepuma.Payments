namespace Firepuma.Payments.Worker.Plumbing.Middleware;

public static class InvalidModelStateLoggingExtensions
{
    public static void AddInvalidModelStateLogging(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.ConfigureApiBehaviorOptions(options =>
        {
            var builtInFactory = options.InvalidModelStateResponseFactory;

            options.InvalidModelStateResponseFactory = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                // Log the invalid ModelState, this is especially useful when Google Pub/Sub push subscriptions calls this API service but
                // the request is invalid and it returns a BadRequest response, the reason gets lost otherwise
                if (!context.ModelState.IsValid)
                {
                    var errors = context.ModelState
                        .Select(result => $"{result.Key}: {string.Join(", ", result.Value?.Errors.Select(err => err.ErrorMessage) ?? new string[] { })}");
                    logger.LogError("ModelState is invalid. {Errors}", string.Join(". ", errors));
                }

                return builtInFactory(context);
            };
        });
    }
}