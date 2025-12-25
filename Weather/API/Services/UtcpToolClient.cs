using API.Models;
using System.Text.Json;

namespace API.Services;

// =============================================================================
// UTCP CLIENT - Consuming tools via UTCP
// =============================================================================

/// <summary>
/// Client for discovering and calling UTCP tools
/// </summary>
public class UtcpToolClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _environmentVariables;
    private UtcpManual? _manual;

    public UtcpToolClient(Dictionary<string, string>? environmentVariables = null)
    {
        _httpClient = new HttpClient();
        _environmentVariables = environmentVariables ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Discover tools from a UTCP manual endpoint
    /// </summary>
    public async Task<UtcpManual> DiscoverToolsAsync(string manualUrl)
    {
        var response = await _httpClient.GetAsync(manualUrl);
        response.EnsureSuccessStatusCode();

        _manual = await response.Content.ReadFromJsonAsync<UtcpManual>()
            ?? throw new Exception("Failed to parse UTCP manual");

        Console.WriteLine($"✅ Discovered {_manual.Tools.Count} tools from {_manual.Name}");
        foreach (var tool in _manual.Tools)
        {
            Console.WriteLine($"   - {tool.Name}: {tool.Description}");
        }

        return _manual;
    }

    /// <summary>
    /// List all available tools
    /// </summary>
    public List<Tool> ListTools()
    {
        if (_manual == null)
            throw new InvalidOperationException("No manual loaded. Call DiscoverToolsAsync first.");

        return _manual.Tools;
    }

    /// <summary>
    /// Get a specific tool by name
    /// </summary>
    public Tool? GetTool(string toolName)
    {
        return _manual?.Tools.FirstOrDefault(t => t.Name == toolName);
    }

    /// <summary>
    /// Call a UTCP tool
    /// </summary>
    public async Task<ToolCallResult> CallToolAsync(string toolName, Dictionary<string, object> parameters)
    {
        if (_manual == null)
            throw new InvalidOperationException("No manual loaded. Call DiscoverToolsAsync first.");

        var tool = GetTool(toolName)
            ?? throw new ArgumentException($"Tool '{toolName}' not found");

        Console.WriteLine($"\n🔧 Calling tool: {toolName}");
        Console.WriteLine($"   Parameters: {JsonSerializer.Serialize(parameters)}");

        if (tool.ToolCallTemplate is HttpCallTemplate httpTemplate)
        {
            return await CallHttpToolAsync(httpTemplate, parameters);
        }

        throw new NotSupportedException($"Call template type not supported: {tool.ToolCallTemplate.CallTemplateType}");
    }

    /// <summary>
    /// Call an HTTP-based UTCP tool
    /// </summary>
    private async Task<ToolCallResult> CallHttpToolAsync(HttpCallTemplate template, Dictionary<string, object> parameters)
    {
        // Build URL with query parameters
        var url = SubstituteVariables(template.Url, parameters);

        if (template.QueryParams != null)
        {
            var queryParams = template.QueryParams
                .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(SubstituteVariables(kvp.Value, parameters))}")
                .ToList();

            if (queryParams.Any())
            {
                url += (url.Contains('?') ? "&" : "?") + string.Join("&", queryParams);
            }
        }

        // Build request
        var request = new HttpRequestMessage(
            new HttpMethod(template.HttpMethod),
            url
        );

        // Add headers
        if (template.Headers != null)
        {
            foreach (var header in template.Headers)
            {
                var value = SubstituteVariables(header.Value, parameters);
                request.Headers.Add(header.Key, value);
            }
        }

        // Add auth headers
        if (_manual?.Auth != null)
        {
            AddAuthToRequest(request, _manual.Auth);
        }

        // Add body for POST/PUT/PATCH
        if (template.Body != null && (template.HttpMethod == "POST" || template.HttpMethod == "PUT" || template.HttpMethod == "PATCH"))
        {
            var bodyJson = JsonSerializer.Serialize(template.Body);
            bodyJson = SubstituteVariables(bodyJson, parameters);
            request.Content = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");
        }

        Console.WriteLine($"   🌐 {template.HttpMethod} {url}");

        // Execute request
        var startTime = DateTime.UtcNow;
        var response = await _httpClient.SendAsync(request);
        var duration = DateTime.UtcNow - startTime;

        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"   ✓ Response received in {duration.TotalMilliseconds:F0}ms");
        Console.WriteLine($"   Status: {(int)response.StatusCode} {response.StatusCode}");

        return new ToolCallResult
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Content = content,
            Duration = duration,
            Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
        };
    }

    /// <summary>
    /// Add authentication to HTTP request
    /// </summary>
    private void AddAuthToRequest(HttpRequestMessage request, AuthConfig auth)
    {
        if (auth.AuthType == "api_key" && auth.ApiKey != null && auth.VarName != null)
        {
            var apiKey = SubstituteVariables(auth.ApiKey, new Dictionary<string, object>());

            if (auth.Location == "header")
            {
                request.Headers.Add(auth.VarName, apiKey);
            }
            else if (auth.Location == "query")
            {
                var uri = new UriBuilder(request.RequestUri!);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                query[auth.VarName] = apiKey;
                uri.Query = query.ToString();
                request.RequestUri = uri.Uri;
            }
        }
    }

    /// <summary>
    /// Substitute template variables with actual values
    /// Format: ${variable_name}
    /// </summary>
    private string SubstituteVariables(string template, Dictionary<string, object> parameters)
    {
        var result = template;

        // Substitute environment variables
        foreach (var kvp in _environmentVariables)
        {
            result = result.Replace($"${{{kvp.Key}}}", kvp.Value);
        }

        // Substitute parameters
        foreach (var kvp in parameters)
        {
            var value = kvp.Value?.ToString() ?? string.Empty;
            result = result.Replace($"${{{kvp.Key}}}", value);
        }

        return result;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Result of a tool call
/// </summary>
public class ToolCallResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Content { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();

    public T? GetContent<T>()
    {
        if (string.IsNullOrWhiteSpace(Content))
            return default;

        return JsonSerializer.Deserialize<T>(Content);
    }
}
