using System.Collections.Concurrent;

namespace ViteDotNet;

/// <summary>
/// A singleton store for the production integration manifests (<c>manifest.prod.json</c>) extracted
/// from each integrated Vite app. Once a manifest has been extracted for a given app directory it is
/// cached here so the values can be read anywhere in the backend without touching the file system again.
/// </summary>
public interface IProdManifestCache
{
    /// <summary>
    /// Returns the cached prod manifest for <paramref name="appDirectory"/>, or <c>null</c> when nothing
    /// has been cached for that app yet.
    /// </summary>
    ProdManifestModel? Get(string appDirectory);

    /// <summary>
    /// Attempts to read the cached prod manifest for <paramref name="appDirectory"/>.
    /// </summary>
    bool TryGet(string appDirectory, out ProdManifestModel? manifest);

    /// <summary>
    /// Adds or replaces the cached prod manifest for <paramref name="appDirectory"/>.
    /// </summary>
    void Set(string appDirectory, ProdManifestModel manifest);
}

/// <inheritdoc />
public class ProdManifestCache : IProdManifestCache
{
    private readonly ConcurrentDictionary<string, ProdManifestModel> _cache = new();

    /// <inheritdoc />
    public ProdManifestModel? Get(string appDirectory) =>
        _cache.TryGetValue(appDirectory, out var manifest) ? manifest : null;

    /// <inheritdoc />
    public bool TryGet(string appDirectory, out ProdManifestModel? manifest)
    {
        var found = _cache.TryGetValue(appDirectory, out var value);
        manifest = value;
        return found;
    }

    /// <inheritdoc />
    public void Set(string appDirectory, ProdManifestModel manifest) =>
        _cache[appDirectory] = manifest;
}
