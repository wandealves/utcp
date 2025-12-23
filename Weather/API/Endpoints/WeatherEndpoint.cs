using API.Services;

namespace API.Endpoints;

public static class WeatherEndpoint
{
    static string[] summaries = new[]
  {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
    public static void RegistrarWeatherEndpoints(this IEndpointRouteBuilder routes)
    {
        var chat = routes.MapGroup("/api/v1/weatherforecast")
            .WithName("Weather")
            .WithTags("Weather");

        chat.MapGet("/{city}", async (string city) =>
        {
            if (city.Equals("Unknown")) 
            {
                return Enumerable.Empty<WeatherForecast>();
            }
            var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)],
                city
            ))
            .ToArray();
            return forecast;
        })
        .WithName("Get Weather");
    }
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary, string? city)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

