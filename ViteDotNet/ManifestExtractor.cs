using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ViteDotNet;

public class ManifestExtractor : IManifestExtractor
{
    private readonly IWebHostEnvironment _environment;
    private readonly Dictionary<string, ManifestModel> ManifestMap = new Dictionary<string, ManifestModel>();

    public ManifestExtractor(IWebHostEnvironment environment, IOptions<Dictionary<string, IntegrationConfigModel>> config)
    {
        _environment = environment;

        foreach (var app in config.Value)
        {
            var appRoot = app.Value.RootDirectory;

            ManifestMap.Add(app.Key, GetManifestFileContent(appRoot));
        }
    }

    public ManifestModel? GetManifestByAppName(string appName)
    {
        ManifestModel? manifest;

        ManifestMap.TryGetValue(appName, out manifest);

        return manifest;
    }

    //TODO: Should be private.
    public ManifestModel GetManifestFileContent(string appFolder)
    {
        var rootPath = _environment.ContentRootPath; //get the root path

        var fullPath = Path.Combine(rootPath, $"wwwroot/{appFolder}/manifest.json");

        var jsonData = File.ReadAllText(fullPath);

        var manifest = JsonSerializer.Deserialize<Dictionary<string, ManifestModel>>(jsonData);

        if(manifest is null)
        {
            throw new ArgumentNullException($"The manifest file in your SPA folder was not found. We searched for it here: {fullPath}");
        }

        var entrypoint = manifest.Single(x => x.Value.isEntry.GetValueOrDefault());

        return entrypoint.Value;
    }
}
