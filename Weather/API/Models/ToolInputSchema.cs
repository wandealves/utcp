using System.Text.Json.Serialization;

namespace API.Models;

public class ToolInputSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, PropertySchema>? Properties { get; set; }

    [JsonPropertyName("required")]
    public string[]? Required { get; set; }
}