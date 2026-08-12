namespace ViteDotNet;

/// <summary>
/// Represents the contents of a <c>manifest.dev.json</c> file emitted by the Vite dev server
/// (via the ViteDotNet Vite plugin) for a single integrated app.
/// </summary>
public record DevManifestModel(int Port, string Entrypoint, string ContainerElementId, bool IsReact);
