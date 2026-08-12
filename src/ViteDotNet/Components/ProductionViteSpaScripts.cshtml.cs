using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechGems.StaticComponents;

namespace ViteDotNet.Components;

/// <summary>
/// Renders the production script/style tags for an integrated Vite app, reading Vite's own
/// <c>manifest.json</c> for the hashed bundle paths and <c>manifest.prod.json</c> for the
/// integration metadata (container element id).
/// </summary>
[HtmlTargetElement("prod-vite-scripts")]
public class ProductionViteSpaScripts : StaticComponent
{
    private readonly IViteConfigService _configService;
    private readonly IManifestExtractor _manifestExtractor;
    private readonly IProdManifestExtractor _prodManifestExtractor;

    /// <summary>
    /// </summary>
    public ProductionViteSpaScripts(
        IViteConfigService configService,
        IManifestExtractor manifestExtractor,
        IProdManifestExtractor prodManifestExtractor
    ) : base()
    {
        _configService = configService;
        _manifestExtractor = manifestExtractor;
        _prodManifestExtractor = prodManifestExtractor;
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
    /// The entry chunk from Vite's production <c>manifest.json</c> (hashed js/css paths).
    /// </summary>
    [HtmlAttributeNotBound]
    public ManifestModel? AppManifest => _manifestExtractor.GetManifestFileContent(AppDirectory);

    /// <summary>
    /// The production integration metadata emitted by the ViteDotNet plugin (<c>manifest.prod.json</c>).
    /// </summary>
    [HtmlAttributeNotBound]
    public ProdManifestModel? ProdManifest => _prodManifestExtractor.Extract(AppDirectory);

    /// <summary>
    /// The container element id the app mounts into, taken from the production integration metadata.
    /// </summary>
    [HtmlAttributeNotBound]
    public string ContainerElementId => ProdManifest?.ContainerElementId ?? "app";
}
