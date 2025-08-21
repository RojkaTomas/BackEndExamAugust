namespace SmartParking_API.Infrastructure;

public static class ErrorHandling
{
    public static void UseGlobalErrorHandling(this IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.Use(async (ctx, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger("GlobalException");
                logger.LogError(ex, "Unhandled exception");
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await ctx.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred"
                });
            }
        });
    }
}
