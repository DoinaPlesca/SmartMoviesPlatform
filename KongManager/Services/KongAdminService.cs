using System.Net.Http;
using System.Text;
using System.Text.Json;
using KongManager.Models;

namespace KongManager.Services;

public class KongAdminService
{
    private readonly HttpClient _http;
    private readonly ILogger<KongAdminService> _logger;

    public KongAdminService(IHttpClientFactory httpClientFactory, ILogger<KongAdminService> logger)
    {
        _http = httpClientFactory.CreateClient("KongAdmin");
        _logger = logger;
    }

    public async Task<string> RegisterServiceAsync(string name, string url)
    {
        var payload = new
        {
            name = name,
            url = url,
            connect_timeout = 60000,
            read_timeout = 60000,
            write_timeout = 60000,
            retries = 5,
            protocol = "http",
            host = new Uri(url).Host,
            port = new Uri(url).Port,
            enabled = true,
            path = new Uri(url).AbsolutePath == "/" ? null : new Uri(url).AbsolutePath
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("/services", content);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to register service {Name}: {Response}", name, result);
        }

        return result;
    }


    public async Task<string?> RegisterRouteAsync(string serviceName, string path, List<string>? methods = null)
    {
        var routePayload = new
        {
            paths = new[] { path },
            preserve_host = false,
            protocols = new[] { "http", "https" },
            https_redirect_status_code = 426,
            request_buffering = true,
            response_buffering = true,
            path_handling = "v0",
            methods = methods?.Select(m => m.ToUpperInvariant()).ToArray()
        };

        var json = JsonSerializer.Serialize(routePayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync($"/services/{serviceName}/routes", content);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(" Failed to register route for {Service} at path {Path}: {Response}", serviceName, path, result);
            return null;
        }

        return result;
    }

    public async Task<string?> GetRouteIdByPathAsync(string path)
    {
        var response = await _http.GetAsync("/routes");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var routes = doc.RootElement.GetProperty("data");

        foreach (var route in routes.EnumerateArray())
        {
            if (route.TryGetProperty("paths", out var pathsElement))
            {
                foreach (var p in pathsElement.EnumerateArray())
                {
                    if (p.GetString() == path)
                    {
                        return route.GetProperty("id").GetString();
                    }
                }
            }
        }

        return null;
    }

    public async Task<bool> PluginAlreadyExistsAsync(string routeId, string pluginName)
    {
        var response = await _http.GetAsync($"/routes/{routeId}/plugins");
        if (!response.IsSuccessStatusCode) return false;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var plugins = doc.RootElement.GetProperty("data");

        return plugins.EnumerateArray()
            .Any(plugin => plugin.GetProperty("name").GetString() == pluginName);
    }

    public async Task AddPluginToRouteAsync(string routeId, PluginConfig plugin)
    {
        if (await PluginAlreadyExistsAsync(routeId, plugin.Name))
        {
            _logger.LogInformation("{Plugin} already exists on route {RouteId}", plugin.Name, routeId);
            return;
        }

        var body = new
        {
            name = plugin.Name,
            config = plugin.Config
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _http.PostAsync($"/routes/{routeId}/plugins", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Failed to add plugin {Plugin} to route {RouteId}. Response: {ResponseBody}. Sent: {JsonBody}",
                plugin.Name, routeId, responseBody, json);

            response.EnsureSuccessStatusCode();
        }
        else
        {
            _logger.LogInformation("{Plugin} plugin added to route {RouteId}", plugin.Name, routeId);
        }
    }
    
    public async Task DeleteAllRoutesAsync()
    {
        var response = await _http.GetAsync("/routes");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch routes: {Response}", await response.Content.ReadAsStringAsync());
            return;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var routes = doc.RootElement.GetProperty("data");

        foreach (var route in routes.EnumerateArray())
        {
            var id = route.GetProperty("id").GetString();
            var deleteResponse = await _http.DeleteAsync($"/routes/{id}");

            if (!deleteResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to delete route {Id}: {Response}", id, await deleteResponse.Content.ReadAsStringAsync());
            }
            else
            {
                _logger.LogInformation("Deleted route {Id}", id);
            }
        }
    }
    public async Task DeleteAllServicesAsync()
    {
        var response = await _http.GetAsync("/services");
        if (!response.IsSuccessStatusCode) return;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var services = doc.RootElement.GetProperty("data");

        foreach (var svc in services.EnumerateArray())
        {
            var id = svc.GetProperty("id").GetString();
            if (!string.IsNullOrEmpty(id))
            {
                await _http.DeleteAsync($"/services/{id}");
            }
        }
    }
    
    // public async Task EnableGlobalFileLogAsync(string logPath = "/dev/stdout")
    // {
    //     var payload = new
    //     {
    //         name = "file-log",
    //         config = new
    //         {
    //             path = logPath
    //         }
    //     };
    //
    //     var json = JsonSerializer.Serialize(payload);
    //     var content = new StringContent(json, Encoding.UTF8, "application/json");
    //
    //     var response = await _http.PostAsync("/plugins", content);
    //     var result = await response.Content.ReadAsStringAsync();
    //
    //     if (!response.IsSuccessStatusCode)
    //     {
    //         _logger.LogError(" Failed to enable global file-log plugin: {Response}", result);
    //     }
    //     else
    //     {
    //         _logger.LogInformation("Global file-log plugin enabled. Logging to container stdout.");
    //     }
    // }



}
