using System.Text.Json.Serialization;

namespace API.Models;

public class HttpCallTemplate
{
    [JsonPropertyName("call_template_type")]
    public string CallTemplateType { get; set; } = "http";
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("http_method")]
    public string HttpMethod { get; set; } = "GET";

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    [JsonPropertyName("query_params")]
    public Dictionary<string, string>? QueryParams { get; set; }

    [JsonPropertyName("body")]
    public object? Body { get; set; }
}
