using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace ViteDotNet;

/// <inheritdoc />
public class ManifestExtractor : IManifestExtractor
{
    private readonly IWebHostEnvironment _environment;

    public ManifestExtractor(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <inheritdoc />
    public ManifestModel? GetManifestFileContent(string appFolder)
    {
        var rootPath = _environment.ContentRootPath; //get the root path

        var fullPath = Path.Combine(rootPath, $"wwwroot/{appFolder}/manifest.json");

        if (!File.Exists(fullPath))
        {
            return null;
        }

        var jsonData = File.ReadAllText(fullPath);

        var manifest = JsonSerializer.Deserialize<Dictionary<string, ManifestModel>>(jsonData);

        if (manifest is null)
        {
            throw new ArgumentNullException($"The manifest file in your SPA folder was not found in the following path: {fullPath}. When using the <prod-vite-scripts /> tag helper, make sure that a production build has been created.");
        }

        var entrypoint = manifest.Single(x => x.Value.isEntry.GetValueOrDefault());

        return entrypoint.Value;
    }
}
