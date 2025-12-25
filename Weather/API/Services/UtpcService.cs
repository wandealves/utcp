using API.Models;
using OpenAI.Chat;
using System.Text.Json;

namespace API.Services;

public interface IUtcpService
{
    /// <summary>
    /// Discover tools from UTCP manual endpoint
    /// </summary>
    Task DiscoverToolsAsync(string manualUrl);

    /// <summary>
    /// Get all discovered tools in OpenAI function calling format
    /// </summary>
    IEnumerable<ChatTool> GetToolsForOpenAI();

    /// <summary>
    /// Execute a UTCP tool by name with JSON parameters
    /// </summary>
    Task<object> ExecuteAsync(string toolName, JsonElement arguments);

    /// <summary>
    /// Execute a UTCP tool by name with dictionary parameters
    /// </summary>
    Task<object> ExecuteAsync(string toolName, Dictionary<string, object> arguments);
}

/// <summary>
/// UTCP Service that bridges UTCP tools with OpenAI function calling
/// </summary>
public class UtcpService : IUtcpService, IDisposable
{
    private readonly UtcpToolClient _utcpClient;
    private readonly Dictionary<string, ChatTool> _openAITools = new();
    private bool _toolsDiscovered = false;

    public UtcpService(IConfiguration configuration)
    {
        // Load environment variables for UTCP auth (API keys, etc)
        var environmentVariables = new Dictionary<string, string>();

        // You can configure these in appsettings.json or environment variables
        var utcpConfig = configuration.GetSection("UTCP");
        foreach (var item in utcpConfig.GetChildren())
        {
            environmentVariables[item.Key] = item.Value ?? string.Empty;
        }

        _utcpClient = new UtcpToolClient(environmentVariables);
    }

    /// <summary>
    /// Discover tools from UTCP manual endpoint
    /// </summary>
    public async Task DiscoverToolsAsync(string manualUrl)
    {
        var manual = await _utcpClient.DiscoverToolsAsync(manualUrl);

        // Convert UTCP tools to OpenAI ChatTool format
        _openAITools.Clear();

        foreach (var tool in manual.Tools)
        {
            var openAITool = ConvertUtcpToolToOpenAI(tool);
            _openAITools[tool.Name] = openAITool;
        }

        _toolsDiscovered = true;
    }

    /// <summary>
    /// Get all discovered tools in OpenAI function calling format
    /// </summary>
    public IEnumerable<ChatTool> GetToolsForOpenAI()
    {
        if (!_toolsDiscovered)
        {
            throw new InvalidOperationException(
                "Tools not discovered yet. Call DiscoverToolsAsync first."
            );
        }

        return _openAITools.Values;
    }

    /// <summary>
    /// Execute a UTCP tool by name with JSON parameters
    /// </summary>
    public async Task<object> ExecuteAsync(string toolName, JsonElement arguments)
    {
        var parameters = JsonElementToDictionary(arguments);
        return await ExecuteAsync(toolName, parameters);
    }

    /// <summary>
    /// Execute a UTCP tool by name with dictionary parameters
    /// </summary>
    public async Task<object> ExecuteAsync(string toolName, Dictionary<string, object> arguments)
    {
        var result = await _utcpClient.CallToolAsync(toolName, arguments);

        if (!result.Success)
        {
            return new
            {
                error = true,
                status_code = result.StatusCode,
                message = $"Tool '{toolName}' failed with status {result.StatusCode}",
                details = result.Content
            };
        }

        // Try to parse JSON response, otherwise return as string
        try
        {
            return JsonSerializer.Deserialize<object>(result.Content)
                ?? result.Content;
        }
        catch
        {
            return result.Content;
        }
    }

    /// <summary>
    /// Convert UTCP tool definition to OpenAI ChatTool format
    /// </summary>
    private ChatTool ConvertUtcpToolToOpenAI(Tool utcpTool)
    {
        // Build the function definition
        var functionDefinition = ChatTool.CreateFunctionTool(
            functionName: utcpTool.Name,
            functionDescription: utcpTool.Description
        );

        // Convert UTCP input schema to OpenAI parameters format
        if (utcpTool.Inputs?.Properties != null)
        {
            var parametersJson = new
            {
                type = utcpTool.Inputs.Type,
                properties = utcpTool.Inputs.Properties.ToDictionary(
                    kvp => kvp.Key,
                    kvp => ConvertPropertySchema(kvp.Value)
                ),
                required = utcpTool.Inputs.Required ?? Array.Empty<string>()
            };

            // Serialize to JSON and set as parameters
            var parametersJsonString = JsonSerializer.Serialize(parametersJson);

            // Use reflection to set the parameters (since OpenAI SDK may not expose direct setter)
            // Alternative: Create the ChatTool with proper JSON string
            functionDefinition = ChatTool.CreateFunctionTool(
                functionName: utcpTool.Name,
                functionDescription: utcpTool.Description,
                functionParameters: BinaryData.FromString(parametersJsonString)
            );
        }

        return functionDefinition;
    }

    /// <summary>
    /// Convert UTCP PropertySchema to OpenAI property format
    /// </summary>
    private object ConvertPropertySchema(PropertySchema schema)
    {
        var property = new Dictionary<string, object>
        {
            ["type"] = schema.Type
        };

        if (!string.IsNullOrEmpty(schema.Description))
        {
            property["description"] = schema.Description;
        }

        if (schema.Enum != null && schema.Enum.Length > 0)
        {
            property["enum"] = schema.Enum;
        }

        if (schema.Minimum.HasValue)
        {
            property["minimum"] = schema.Minimum.Value;
        }

        if (schema.Maximum.HasValue)
        {
            property["maximum"] = schema.Maximum.Value;
        }

        return property;
    }

    /// <summary>
    /// Convert JsonElement to Dictionary for UTCP client
    /// </summary>
    private Dictionary<string, object> JsonElementToDictionary(JsonElement element)
    {
        var dictionary = new Dictionary<string, object>();

        if (element.ValueKind != JsonValueKind.Object)
        {
            return dictionary;
        }

        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = ConvertJsonElement(property.Value);
        }

        return dictionary;
    }

    /// <summary>
    /// Convert JsonElement to appropriate CLR type
    /// </summary>
    private object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray()
                .Select(ConvertJsonElement)
                .ToArray(),
            JsonValueKind.Object => JsonElementToDictionary(element),
            _ => element.ToString()
        };
    }

    public void Dispose()
    {
        _utcpClient?.Dispose();
    }
}