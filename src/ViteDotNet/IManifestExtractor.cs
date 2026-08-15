namespace ViteDotNet;

/// <summary>
/// Reads Vite's production <c>manifest.json</c> for an integrated app and returns the entry chunk.
/// </summary>
public interface IManifestExtractor
{
    /// <summary>
    /// Reads <c>wwwroot/&lt;appFolder&gt;/manifest.json</c> and returns the entry chunk, or <c>null</c>
    /// when no manifest file exists (e.g. a production build has not been created).
    /// </summary>
    ManifestModel? GetManifestFileContent(string appFolder);
}
