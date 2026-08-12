namespace ViteDotNet;

/// <summary>
/// A singleton store for the resolved Vite.NET configuration. The backend configuration has been
/// reduced to just the directory name(s) of the integrated Vite app(s); every other value
/// (entrypoint, container element id, port, whether the app is React) now flows from the manifests
/// emitted by the Vite plugin. This service resolves which app directory a tag helper should use.
/// </summary>
public interface IViteConfigService
{
    /// <summary>
    /// The configured Vite app directory names.
    /// </summary>
    IReadOnlyList<string> AppDirectories { get; }

    /// <summary>
    /// Whether exactly one app directory has been configured, in which case the app name may be
    /// omitted on the tag helpers.
    /// </summary>
    bool HasSingleApp { get; }

    /// <summary>
    /// Resolves the app directory to use. When a single app is configured its directory is returned
    /// and <paramref name="appName"/> is optional; otherwise the directory matching
    /// <paramref name="appName"/> is returned.
    /// </summary>
    /// <param name="appName">The directory name of the app to resolve. Required when more than one app is configured.</param>
    string GetAppDirectory(string? appName = null);
}

/// <inheritdoc />
public class ViteConfigService : IViteConfigService
{
    /// <inheritdoc />
    public IReadOnlyList<string> AppDirectories { get; }

    /// <summary>
    /// Reads the configured directory name(s) once and stores them for the lifetime of the app.
    /// </summary>
    /// <param name="appDirectories">
    /// The app directory names bound from the <c>ViteDotNet</c> configuration section, which may be
    /// a single directory string or an array of directory strings.
    /// </param>
    public ViteConfigService(IEnumerable<string> appDirectories)
    {
        AppDirectories = appDirectories
            .Where(directory => !string.IsNullOrWhiteSpace(directory))
            .ToList();
    }

    /// <inheritdoc />
    public bool HasSingleApp => AppDirectories.Count == 1;

    /// <inheritdoc />
    public string GetAppDirectory(string? appName = null)
    {
        if (AppDirectories.Count == 0)
        {
            throw new InvalidOperationException("No Vite app directories were found in the configuration. Add a directory name (or an array of directory names) under the \"ViteDotNet\" section.");
        }

        if (string.IsNullOrWhiteSpace(appName))
        {
            if (HasSingleApp)
            {
                return AppDirectories[0];
            }

            throw new InvalidOperationException("No application name was provided and more than one Vite app directory is configured. Specify the directory via the \"app-name\" attribute.");
        }

        var match = AppDirectories.FirstOrDefault(directory =>
            string.Equals(directory, appName, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            throw new InvalidOperationException($"The application directory \"{appName}\" was not found in the configuration.");
        }

        return match;
    }
}
