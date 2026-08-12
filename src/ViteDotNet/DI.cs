using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ViteDotNet;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds the Vite.NET integration, reading the app directory name(s) from the <c>ViteDotNet</c>
    /// configuration section (either a single directory string or an array of directory strings).
    /// </summary>
    public static void AddViteIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        var appDirectories = ReadAppDirectories(configuration);
        AddViteIntegration(services, appDirectories);
    }

    /// <summary>
    /// Adds the Vite.NET integration for a single Vite app directory.
    /// </summary>
    /// <param name="appDirectory">The directory name of the integrated Vite app (e.g. <c>"ReactApp"</c>).</param>
    public static void AddViteIntegration(this IServiceCollection services, string appDirectory)
    {
        AddViteIntegration(services, new[] { appDirectory });
    }

    /// <summary>
    /// Adds the Vite.NET integration for one or more Vite app directories.
    /// </summary>
    /// <param name="services">The service collection to add the integration to.</param>
    /// <param name="appDirectories">The directory names of the integrated Vite apps.</param>
    public static void AddViteIntegration(this IServiceCollection services, IEnumerable<string> appDirectories)
    {
        services.AddSingleton<IViteConfigService>(_ => new ViteConfigService(appDirectories));

        services.AddSingleton<IManifestExtractor, ManifestExtractor>();
        services.AddSingleton<IDevManifestCache, DevManifestCache>();
        services.AddSingleton<IDevManifestExtractor, DevManifestExtractor>();
        services.AddSingleton<IProdManifestCache, ProdManifestCache>();
        services.AddSingleton<IProdManifestExtractor, ProdManifestExtractor>();
    }

    /// <summary>
    /// Reads the app directory name(s) from the <c>ViteDotNet</c> configuration section, supporting
    /// either a single directory string (<c>"ViteDotNet": "ReactApp"</c>) or an array of directory
    /// strings (<c>"ViteDotNet": [ "ReactApp", "SvelteApp" ]</c>).
    /// </summary>
    private static IReadOnlyList<string> ReadAppDirectories(IConfiguration configuration)
    {
        var section = configuration.GetSection("ViteDotNet");

        // Array form: the section has indexed children ("ViteDotNet:0", "ViteDotNet:1", ...).
        var arrayValues = section.GetChildren()
            .Select(child => child.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();

        if (arrayValues.Count > 0)
        {
            return arrayValues;
        }

        // Single string form.
        var singleValue = section.Value;
        if (!string.IsNullOrWhiteSpace(singleValue))
        {
            return new[] { singleValue };
        }

        return Array.Empty<string>();
    }
}
