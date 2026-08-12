using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace ViteDotNet;

/// <summary>
/// Extracts the <c>manifest.dev.json</c> emitted by the Vite dev server for an integrated app.
/// The app is identified purely by its directory name, and the manifest is always expected at
/// <c>wwwroot/&lt;app directory&gt;/manifest.dev.json</c> (alongside the production build output).
/// Extracted values are stored in the <see cref="IDevManifestCache"/> singleton so they can be
/// read anywhere in the backend.
/// </summary>
public interface IDevManifestExtractor
{
    /// <summary>
    /// Extracts the dev manifest for the app in <paramref name="appDirectory"/>. When the manifest has
    /// already been cached the cached value is returned; otherwise it is read from disk, cached and
    /// returned. Returns <c>null</c> when no manifest file exists (e.g. the dev server has not started yet).
    /// </summary>
    DevManifestModel? Extract(string appDirectory);
}

/// <inheritdoc />
public class DevManifestExtractor : IDevManifestExtractor
{
    private const string DevManifestFileName = "manifest.dev.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IWebHostEnvironment _environment;
    private readonly IDevManifestCache _cache;

    public DevManifestExtractor(IWebHostEnvironment environment, IDevManifestCache cache)
    {
        _environment = environment;
        _cache = cache;
    }

    /// <inheritdoc />
    public DevManifestModel? Extract(string appDirectory)
    {
        if (string.IsNullOrWhiteSpace(appDirectory))
        {
            throw new ArgumentException("An app directory name must be provided.", nameof(appDirectory));
        }

        if (_cache.TryGet(appDirectory, out var cached) && cached is not null)
        {
            return cached;
        }

        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRootPath, appDirectory, DevManifestFileName);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        var jsonData = File.ReadAllText(fullPath);

        var manifest = JsonSerializer.Deserialize<DevManifestModel>(jsonData, SerializerOptions);

        if (manifest is null)
        {
            throw new InvalidOperationException(
                $"The dev manifest at {fullPath} could not be deserialized. Make sure the Vite dev server is running and has emitted a valid {DevManifestFileName}.");
        }

        _cache.Set(appDirectory, manifest);

        return manifest;
    }
}
