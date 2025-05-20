using System.Text.Json;

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
        _logger.LogInformation("Registering service: {Name} → {Url}", name, url);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("name", name),
            new KeyValuePair<string, string>("url", url)
        });

        var response = await _http.PostAsync("/services", content);
        var result = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Service registration response: {Response}", result);
        return result;
    }

    
    public async Task<string> RegisterRouteAsync(string serviceName, string path)
    {
        _logger.LogInformation("Registering route for service {Service} at path {Path}", serviceName, path);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("paths[]", path)
        });

        var response = await _http.PostAsync($"/services/{serviceName}/routes", content);
        var result = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Route registration response: {Response}", result);
        return result;
    }

    
    public async Task<string?> GetRouteIdByPathAsync(string path)
    {
        _logger.LogInformation(" Looking for route ID by path: {Path}", path);

        var response = await _http.GetAsync("/routes");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch routes from Kong. Status: {Status}", response.StatusCode);
            return null;
        }

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
                        var id = route.GetProperty("id").GetString();
                        _logger.LogInformation(" Found route ID: {Id} for path: {Path}", id, path);
                        return id;
                    }
                }
            }
        }

        _logger.LogWarning(" No route ID found for path: {Path}", path);
        return null;
    }


    public async Task AddHttpLogPluginToRouteAsync(string routeId)
    {
        _logger.LogInformation(" Attaching http-log plugin to route: {RouteId}", routeId);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("name", "http-log"),
            new KeyValuePair<string, string>("config.http_endpoint", "http://httpbin.org/post"),
            new KeyValuePair<string, string>("config.method", "POST"),
            new KeyValuePair<string, string>("config.timeout", "1000"),
            new KeyValuePair<string, string>("config.keepalive", "1000")
        });

        var response = await _http.PostAsync($"/routes/{routeId}/plugins", content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(" http-log plugin attached successfully to route {RouteId}", routeId);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(" Failed to attach http-log plugin to route {RouteId}. Status: {Status}. Response: {Response}",
                routeId, response.StatusCode, errorContent);
            response.EnsureSuccessStatusCode(); // still throw to avoid silent failures
        }
    }

    
    //60 requests per minute per client based on Kong’s in-memory counter
    public async Task AddRateLimitPluginToRouteAsync(string routeId, int limitPerMinute = 60)
    {
        _logger.LogInformation(" Attaching rate-limiting plugin to route {RouteId} with limit {Limit} req/min",
            routeId, limitPerMinute);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("name", "rate-limiting"),
            new KeyValuePair<string, string>("config.minute", limitPerMinute.ToString()),
            new KeyValuePair<string, string>("config.policy", "local")
        });

        var response = await _http.PostAsync($"/routes/{routeId}/plugins", content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(" Rate limiting plugin successfully added to route {RouteId}", routeId);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(" Failed to add rate-limiting plugin to route {RouteId}. Status: {Status}. Response: {Response}",
                routeId, response.StatusCode, error);
            response.EnsureSuccessStatusCode(); // Still throw to stop bad deployments
        }
    }

   
    public async Task AddJwtPluginToRouteAsync(string routeId)
    {
        _logger.LogInformation("Attaching JWT plugin to route: {RouteId}", routeId);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("name", "jwt"),
            new KeyValuePair<string, string>("config.secret_is_base64", "false"),
            new KeyValuePair<string, string>("config.claims_to_verify", "exp")
        });

        var response = await _http.PostAsync($"/routes/{routeId}/plugins", content);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("JWT plugin attached to route: {RouteId}", routeId);
    }

    
    public async Task AddCorsPluginToRouteAsync(string routeId)
    {
        _logger.LogInformation(" Attaching CORS plugin to route: {RouteId}", routeId);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("name", "cors"),
            new KeyValuePair<string, string>("config.origins", "*"),
            new KeyValuePair<string, string>("config.methods", "GET, POST, PUT, DELETE, OPTIONS"),
            new KeyValuePair<string, string>("config.headers", "Authorization, Content-Type"),
            new KeyValuePair<string, string>("config.exposed_headers", "Authorization"),
            new KeyValuePair<string, string>("config.credentials", "true"),
            new KeyValuePair<string, string>("config.max_age", "3600")
        });

        var response = await _http.PostAsync($"/routes/{routeId}/plugins", content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(" CORS plugin successfully added to route {RouteId}", routeId);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(" Failed to attach CORS plugin to route {RouteId}. Status: {Status}. Response: {Response}",
                routeId, response.StatusCode, error);
            response.EnsureSuccessStatusCode(); // Still stop execution on failure
        }
    }


    public async Task<string> CreateConsumerAsync(string username)
    {
        _logger.LogInformation(" Creating Kong consumer: {Username}", username);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", username)
        });

        var response = await _http.PostAsync("/consumers", content);
        var result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(" Consumer '{Username}' created successfully", username);
        }
        else
        {
            _logger.LogError(" Failed to create consumer '{Username}'. Status: {Status}. Response: {Response}",
                username, response.StatusCode, result);
            response.EnsureSuccessStatusCode();
        }

        return result;
    }

    public async Task<string> CreateJwtCredentialAsync(string consumerUsername, string key, string secret)
    {
        _logger.LogInformation(" Creating JWT credential for consumer: {Username} (iss: {Key})", consumerUsername, key);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("key", key),
            new KeyValuePair<string, string>("secret", secret)
        });

        var response = await _http.PostAsync($"/consumers/{consumerUsername}/jwt", content);
        var result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(" JWT credential created for consumer '{Username}'", consumerUsername);
        }
        else
        {
            _logger.LogError(" Failed to create JWT credential for '{Username}'. Status: {Status}. Response: {Response}",
                consumerUsername, response.StatusCode, result);
            response.EnsureSuccessStatusCode();
        }

        return result;
    }
}