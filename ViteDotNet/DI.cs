using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ViteDotNet;

public static class DependencyInjectionExtensions
{
    public static void AddViteIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IManifestExtractor, ManifestExtractor>();
        services.Configure<Dictionary<string, IntegrationConfigModel>>(configuration.GetRequiredSection("ViteDotNet"));
    }
}