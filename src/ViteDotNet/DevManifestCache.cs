using System.Collections.Concurrent;

namespace ViteDotNet;

/// <summary>
/// A singleton store for the dev manifests extracted from each integrated Vite app.
/// Once a manifest has been extracted for a given app it is cached here so the values can be
/// read anywhere in the backend without touching the file system again.
/// </summary>
public interface IDevManifestCache
{
    /// <summary>
    /// Returns the cached dev manifest for <paramref name="appName"/>, or <c>null</c> when nothing
    /// has been cached for that app yet.
    /// </summary>
    DevManifestModel? Get(string appName);

    /// <summary>
    /// Attempts to read the cached dev manifest for <paramref name="appName"/>.
    /// </summary>
    bool TryGet(string appName, out DevManifestModel? manifest);

    /// <summary>
    /// Adds or replaces the cached dev manifest for <paramref name="appName"/>.
    /// </summary>
    void Set(string appName, DevManifestModel manifest);
}

/// <inheritdoc />
public class DevManifestCache : IDevManifestCache
{
    private readonly ConcurrentDictionary<string, DevManifestModel> _cache = new();

    /// <inheritdoc />
    public DevManifestModel? Get(string appName) =>
        _cache.TryGetValue(appName, out var manifest) ? manifest : null;

    /// <inheritdoc />
    public bool TryGet(string appName, out DevManifestModel? manifest)
    {
        var found = _cache.TryGetValue(appName, out var value);
        manifest = value;
        return found;
    }

    /// <inheritdoc />
    public void Set(string appName, DevManifestModel manifest) =>
        _cache[appName] = manifest;
}
