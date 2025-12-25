using System.Text.Json.Serialization;

namespace API.Models;
public class Tool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("inputs")]
    public ToolInputSchema Inputs { get; set; } = new();

    [JsonPropertyName("output")]
    public ToolOutputSchema? Output { get; set; }

    [JsonPropertyName("average_response_size")]
    public int? AverageResponseSize { get; set; }

    [JsonPropertyName("tool_call_template")]
    public HttpCallTemplate ToolCallTemplate { get; set; } = new HttpCallTemplate();
}
