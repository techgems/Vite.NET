using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechGems.RazorComponentTagHelpers;

namespace ViteDotNet.Views;

[HtmlTargetElement("prod-vite-scripts")]
public class ProductionViteScriptsComponent : RazorComponentTagHelper
{
    private readonly IntegrationConfigModel _simpleAppConfig;
    private readonly Dictionary<string, IntegrationConfigModel> _apps;
    private readonly IManifestExtractor _manifestExtractor;

    public ProductionViteScriptsComponent(
        IOptions<Dictionary<string, IntegrationConfigModel>> config,
        IOptions<IntegrationConfigModel> simpleConfig,
        IManifestExtractor manifestExtractor
    ) : base("~/Views/ViteDotNet/ProductionViteSpaScripts.cshtml")
    {
        _apps = config.Value;
        _simpleAppConfig = simpleConfig.Value;
        _manifestExtractor = manifestExtractor;
    }

    /// <summary>
    /// To let the tag helper know to not use the dictionary when an unnamed app config is used.
    /// </summary>
    [HtmlAttributeNotBound]
    private bool UseSimpleConfig
    {
        get
        {
            return _simpleAppConfig is not null;
        }
    }

    [HtmlAttributeName("app-name")]
    public string AppName { get; set; } = string.Empty;

    [HtmlAttributeNotBound]
    public IntegrationConfigModel IntegrationConfig
    {
        get
        {
            if (UseSimpleConfig)
            {
                return _simpleAppConfig;
            }

            if (string.IsNullOrWhiteSpace(AppName))
            {
                throw new InvalidOperationException("No application name was provided.");
            }

            if (!_apps.ContainsKey(AppName))
            {
                throw new InvalidOperationException("The application requested was not found in the configuration.");
            }

            return _apps[AppName];
        }
    }

    [HtmlAttributeNotBound]
    public ManifestModel AppManifest
    {
        get
        {
            var appManifest = _manifestExtractor.GetManifestFileContent(IntegrationConfig.RootDirectory);

            return appManifest;
        }
    }
}
