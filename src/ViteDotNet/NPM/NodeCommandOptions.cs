namespace ViteDotNet.NPM;

/// <summary>
/// Configures a single node command (typically the Vite dev server) that ASP.NET Core launches on
/// startup. Everything here has a sensible default, so the common case is to configure nothing and
/// let the values be derived from the existing <c>ViteDotNet</c> configuration.
/// </summary>
internal class NodeCommandOptions
{
    /// <summary>
    /// The directory name of the Vite app whose dev server should be launched (e.g. <c>"ReactApp"</c>).
    /// Leave <c>null</c> when a single app is configured — the one configured directory is used.
    /// Required when more than one app is configured so the correct one can be resolved.
    /// </summary>
    internal string? AppName { get; set; }

    /// <summary>
    /// The package.json script to run. Defaults to <c>"dev"</c>.
    /// </summary>
    internal string ScriptName { get; set; } = "dev";

    /// <summary>
    /// The package manager executable used to run the script. Defaults to <c>"npm"</c>.
    /// </summary>
    internal string PackageManager { get; set; } = "npm";

    /// <summary>
    /// Overrides the directory the command runs in. When <c>null</c> (the default) the working
    /// directory is derived as <c>ContentRootPath / &lt;app directory&gt;</c>, using the app directory
    /// resolved from <see cref="AppName"/> and the <c>ViteDotNet</c> configuration.
    /// </summary>
    internal string? WorkingDirectory { get; set; }

    /// <summary>
    /// A regular expression matched against the command's standard output to detect that the dev
    /// server has finished starting. This is used only to surface startup failures; the process keeps
    /// running afterwards. Defaults to Vite's <c>"ready in"</c> banner.
    /// </summary>
    internal string ReadyMatch { get; set; } = "ready in";

    /// <summary>
    /// The regex evaluation timeout for <see cref="ReadyMatch"/>. This is a development-time only
    /// feature, so a generous timeout is fine.
    /// </summary>
    internal TimeSpan ReadyMatchTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Extra environment variables to pass to the launched process. Empty by default; the Vite plugin
    /// picks its own port and records it in <c>manifest.dev.json</c>, so no port needs to be passed in.
    /// </summary>
    internal IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>();
}
