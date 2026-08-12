using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace ViteDotNet;

/// <summary>
/// Extracts the <c>manifest.prod.json</c> emitted by the ViteDotNet Vite plugin during a production
/// build. The app is identified purely by its directory name, and the manifest is always expected at
/// <c>wwwroot/&lt;app directory&gt;/manifest.prod.json</c> under the content root, alongside Vite's own
/// <c>manifest.json</c>. Extracted values are stored in the <see cref="IProdManifestCache"/> singleton
/// so they can be read anywhere in the backend.
/// </summary>
public interface IProdManifestExtractor
{
    /// <summary>
    /// Extracts the prod manifest for the app in <paramref name="appDirectory"/>. When the manifest has
    /// already been cached the cached value is returned; otherwise it is read from disk, cached and
    /// returned. Returns <c>null</c> when no manifest file exists (e.g. a production build has not been created).
    /// </summary>
    ProdManifestModel? Extract(string appDirectory);
}

/// <inheritdoc />
public class ProdManifestExtractor : IProdManifestExtractor
{
    private const string ProdManifestFileName = "manifest.prod.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IWebHostEnvironment _environment;
    private readonly IProdManifestCache _cache;

    public ProdManifestExtractor(IWebHostEnvironment environment, IProdManifestCache cache)
    {
        _environment = environment;
        _cache = cache;
    }

    /// <inheritdoc />
    public ProdManifestModel? Extract(string appDirectory)
    {
        if (string.IsNullOrWhiteSpace(appDirectory))
        {
            throw new ArgumentException("An app directory name must be provided.", nameof(appDirectory));
        }

        if (_cache.TryGet(appDirectory, out var cached) && cached is not null)
        {
            return cached;
        }

        var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", appDirectory, ProdManifestFileName);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        var jsonData = File.ReadAllText(fullPath);

        var manifest = JsonSerializer.Deserialize<ProdManifestModel>(jsonData, SerializerOptions);

        if (manifest is null)
        {
            throw new InvalidOperationException(
                $"The prod manifest at {fullPath} could not be deserialized. Make sure a production build has been created and emitted a valid {ProdManifestFileName}.");
        }

        _cache.Set(appDirectory, manifest);

        return manifest;
    }
}
