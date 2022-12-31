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
	private readonly Dictionary<string, IntegrationConfigModel> _apps;

    public DevelopmentViteScriptsComponent(
        IOptions<Dictionary<string, IntegrationConfigModel>> config,
        IOptions<IntegrationConfigModel> simpleConfig
    ) : base("~/Views/ViteDotNet/DevelopmentViteSpaScripts.cshtml")
	{
        //TO DO: determine which config to use.

        var x = simpleConfig.Value;
        _apps = config.Value;
    }

    [HtmlAttributeName("app-name")]
    public string AppName { get; set; } = string.Empty;


    [HtmlAttributeNotBound]
    public IntegrationConfigModel IntegrationConfig { 
        get
        {
            if(string.IsNullOrWhiteSpace(AppName))
            {
                throw new InvalidOperationException("No application name was provided.");
            }

            if(!_apps.ContainsKey(AppName))
            {
                throw new InvalidOperationException("The application requested was not found in the configuration.");
            }

            return _apps[AppName];
        } 
    }
}
