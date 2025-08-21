namespace SmartParking_API.Infrastructure;

public static class ApiKeyAuth
{
    public const string HeaderName = "X-Api-Key";

    public static IApplicationBuilder UseApiKeyAuth(this IApplicationBuilder app)
        => app.Use(async (context, next) =>
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out var supplied))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Missing API key",
                    Detail = $"Header '{HeaderName}' is required."
                });
                return;
            }

            var cfg = context.RequestServices.GetRequiredService<IConfiguration>();
            var expected = cfg["Auth:ApiKey"];

            if (string.IsNullOrWhiteSpace(expected) || supplied != expected)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Invalid API key"
                });
                return;
            }

            await next();
        });
}
