using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace API.Services;

public interface IUtpcService
{
    Task<object> ExecuteAsync(string toolName, JsonElement arguments);
}
public class UtpcService(HttpClient http): IUtpcService
{
    public async Task<object> ExecuteAsync(string toolName, JsonElement arguments)
    {
        if (toolName != "get_weather")
            throw new Exception("Tool não suportada");

        // Extrai a cidade dos argumentos
        var city = arguments.TryGetProperty("city", out var cityProp)
            ? cityProp.GetString()
            : "Unknown";

        var response = await http.GetFromJsonAsync<object>($"api/v1/weatherforecast/{city}");
        return response!;
    }
}

