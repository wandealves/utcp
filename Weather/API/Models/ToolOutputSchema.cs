using System.Text.Json.Serialization;

namespace API.Models;

public class ToolOutputSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
