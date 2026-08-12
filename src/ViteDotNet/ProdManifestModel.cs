namespace ViteDotNet;

/// <summary>
/// Represents the contents of a <c>manifest.prod.json</c> file emitted by the ViteDotNet Vite plugin
/// during a production build. Carries the integration metadata that cannot be derived from Vite's own
/// <c>manifest.json</c> (which only maps source files to their hashed bundle output).
/// </summary>
public record ProdManifestModel(string Entrypoint, string ContainerElementId, bool IsReact);
