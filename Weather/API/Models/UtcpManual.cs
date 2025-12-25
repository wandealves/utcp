using System.Text.Json.Serialization;

namespace API.Models;
public class UtcpManual
{
    [JsonPropertyName("manual_version")]
    public string ManualVersion { get; set; } = "1.0.0";

    [JsonPropertyName("utcp_version")]
    public string UtcpVersion { get; set; } = "1.1.0";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("tools")]
    public List<Tool> Tools { get; set; } = new();

    [JsonPropertyName("auth")]
    public AuthConfig? Auth { get; set; }
}


