using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechGems.RazorComponentTagHelpers;

namespace ViteDotNet.Views;

[HtmlTargetElement("dev-vite-scripts")]
public class DevelopmentViteScriptsComponent : RazorComponentTagHelper
{
    private readonly IntegrationConfigModel _simpleAppConfig;
	private readonly Dictionary<string, IntegrationConfigModel> _apps;

    public DevelopmentViteScriptsComponent(
        IOptions<Dictionary<string, IntegrationConfigModel>> config,
        IOptions<IntegrationConfigModel> simpleConfig
    ) : base("~/Views/ViteDotNet/DevelopmentViteSpaScripts.cshtml")
	{
        _simpleAppConfig = simpleConfig.Value;
        _apps = config.Value;
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

    /// <summary>
    /// The integration config to use. Selects a simple config by default, otherwise, uses the AppName to get the right configuration for the Vite App.
    /// </summary>
    [HtmlAttributeNotBound]
    public IntegrationConfigModel IntegrationConfig { 
        get
        {
            if(UseSimpleConfig)
            {
                return _simpleAppConfig;
            }

            if(string.IsNullOrWhiteSpace(AppName))
            {
                throw new InvalidOperationException("No application name was provided.");
            }

            if(!_apps.ContainsKey(AppName))
            {
                throw new InvalidOperationException($"The application name {AppName} used in the tag helper was not found in the configuration.");
            }

            return _apps[AppName];
        } 
    }
}
