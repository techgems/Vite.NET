namespace ViteDotNet;

public interface IManifestExtractor
{
    ManifestModel? GetManifestByAppName(string appName);

    ManifestModel? GetManifestFileContent(string appFolder);
}
