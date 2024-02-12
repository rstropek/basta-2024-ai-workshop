namespace ConferenceBot;

public static class ExceptionHandling
{
    public static IApplicationBuilder UseDefaultExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("An error occurred while processing your request.");
            });
        });

        return app;
    }
}