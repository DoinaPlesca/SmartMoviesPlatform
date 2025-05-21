namespace KongManager.Models;


public class KongConfigRoot
{
    public string _format_version { get; set; } = "3.0";
    public List<KongService> services { get; set; } = new();
}

public class KongService
{
    public string name { get; set; }
    public string url { get; set; }
    public List<KongRoute> routes { get; set; } = new();
}

public class KongRoute
{
    public string name { get; set; }
    public List<string> paths { get; set; } = new();
    public List<string>? methods { get; set; }
    public string? path_handling { get; set; }
    public bool? strip_path { get; set; }
    public int? regex_priority { get; set; }
    public List<PluginConfig>? plugins { get; set; }
}

public class PluginConfig
{
    public string Name { get; set; } = default!;
    public Dictionary<string, object>? Config { get; set; }
}
