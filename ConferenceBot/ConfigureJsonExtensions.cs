using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ConferenceBot;

static class ConfigureJsonExtensions
{
    public static IServiceCollection ConfigureJsonOptions(this IServiceCollection services, IJsonTypeInfoResolver[] jsonTypeInfoResolvers)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            // We specify the type info for JSON serialization throuth TypeInfoResolver.
            // This is required for AOT compilation.
            options.SerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(jsonTypeInfoResolvers);
            // To demonstrate the new Type Info Resolver Chain, we ensure that there
            // is at least one resolver in the chain.
            // Read more at https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8#chain-source-generators.   
            Debug.Assert(options.SerializerOptions.TypeInfoResolverChain.Count > 0);

            // .NET 8 comes with new naming policies for snake_case and kebab-case.
            // Read more at https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8#naming-policies
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        });

        return services;
    }
}