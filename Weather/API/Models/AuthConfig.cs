using System.Text.Json.Serialization;

namespace API.Models;

public class AuthConfig
{
    [JsonPropertyName("auth_type")]
    public string AuthType { get; set; } = "none";

    [JsonPropertyName("api_key")]
    public string? ApiKey { get; set; }

    [JsonPropertyName("var_name")]
    public string? VarName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }
}