using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ConferenceBot;

public static class RateLimitOptionsExtensions
{
    public const string PolicyName = "FixedWindow";

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfigurationSection configuration)
    {
        var options = new RateLimitOptions();
        configuration.Bind(options);
        services.AddRateLimiter(opt =>
        {
            opt.AddFixedWindowLimiter(policyName: PolicyName, rateLimitBuilder =>
            {
                rateLimitBuilder.AutoReplenishment = true;
                rateLimitBuilder.PermitLimit = options.PermitLimit;
                rateLimitBuilder.Window = TimeSpan.FromSeconds(options.Window);
                rateLimitBuilder.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                rateLimitBuilder.QueueLimit = options.QueueLimit;
            });
            opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}

public class RateLimitOptions
{
    public int PermitLimit { get; set; } = 100;
    public int Window { get; set; } = 10;
    public int QueueLimit { get; set; } = 2;
}
