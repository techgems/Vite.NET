namespace ViteDotNet;

public interface IManifestExtractor
{
    ManifestModel? GetManifestByAppName(string appName);

    public ManifestModel GetManifestFileContent(string root);
}
