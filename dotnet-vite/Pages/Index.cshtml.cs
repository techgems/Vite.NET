using ViteDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dotnet_vite.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IManifestExtractor _manifestExtractor;

        public IndexModel(ILogger<IndexModel> logger, IManifestExtractor manifestExtractor)
        {
            _logger = logger;
            _manifestExtractor = manifestExtractor;
        }

        public void OnGet()
        {
            _manifestExtractor.GetManifestFileContent("ClientApp");
        }
    }
}