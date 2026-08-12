using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechGems.StaticComponents;

namespace ViteDotNet.Components;

[HtmlTargetElement("dev-vite-scripts")]
public class DevelopmentViteSpaScripts : StaticComponent
{
    private readonly IViteConfigService _configService;
    private readonly IDevManifestExtractor _devManifestExtractor;

    public DevelopmentViteSpaScripts(IViteConfigService configService, IDevManifestExtractor devManifestExtractor) : base()
    {
        _configService = configService;
        _devManifestExtractor = devManifestExtractor;
    }

    /// <summary>
    /// The directory name of the Vite app to render. Optional when a single app is configured.
    /// </summary>
    [HtmlAttributeName("app-name")]
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// The resolved app directory name, taken from the app-name attribute or the single configured app.
    /// </summary>
    [HtmlAttributeNotBound]
    public string AppDirectory => _configService.GetAppDirectory(string.IsNullOrWhiteSpace(AppName) ? null : AppName);

    /// <summary>
    /// The dev manifest emitted by the running Vite dev server for this app, or <c>null</c> when the
    /// server has not started yet.
    /// </summary>
    [HtmlAttributeNotBound]
    public DevManifestModel? DevManifest => _devManifestExtractor.Extract(AppDirectory);

    /// <summary>
    /// The dev server port reported by the running Vite dev server.
    /// </summary>
    [HtmlAttributeNotBound]
    public int Port => DevManifest?.Port ?? 5173;

    /// <summary>
    /// The app entrypoint reported by the running Vite dev server.
    /// </summary>
    [HtmlAttributeNotBound]
    public string Entrypoint => DevManifest?.Entrypoint ?? string.Empty;

    /// <summary>
    /// The container element id reported by the running Vite dev server.
    /// </summary>
    [HtmlAttributeNotBound]
    public string ContainerElementId => DevManifest?.ContainerElementId ?? "app";

    /// <summary>
    /// Whether the app is a React app, as detected by the running Vite dev server.
    /// </summary>
    [HtmlAttributeNotBound]
    public bool IsReact => DevManifest?.IsReact ?? false;
}
