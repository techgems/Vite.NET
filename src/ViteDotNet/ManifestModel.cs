namespace ViteDotNet;

public record ManifestModel(string file, string src, bool? isEntry, List<string>? css, List<string>? assets);
