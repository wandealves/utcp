using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class WeatherEndpoint
{
    public static void RegistrarWeatherEndpoints(this IEndpointRouteBuilder routes)
    {
        var app = routes.MapGroup("/api/v1/weatherforecast")
            .WithName("Weather")
            .WithTags("Weather");

        // Actual Weather Endpoint
        app.MapGet("/api/v1/current", ([FromQuery] string location, [FromQuery] string units = "celsius") =>
        {
            // Simulação de dados de clima
            var weather = new
            {
                location = location,
                timestamp = DateTime.UtcNow,
                temperature = units == "celsius" ? 22.5 : 72.5,
                units = units,
                conditions = "Partly Cloudy",
                humidity = 65,
                windSpeed = 12.5,
                windDirection = "NW"
            };

            return Results.Ok(weather);
        })
        .WithName("GetCurrentWeather");

        // Forecast Endpoint
        app.MapGet("/api/v1/forecast", ([FromQuery] string location, [FromQuery] int days = 3) =>
        {
            var forecast = new List<object>();

            for (int i = 0; i < Math.Min(days, 7); i++)
            {
                forecast.Add(new
                {
                    date = DateTime.UtcNow.AddDays(i).ToString("yyyy-MM-dd"),
                    location = location,
                    tempMax = 25 + i,
                    tempMin = 15 + i,
                    conditions = i % 2 == 0 ? "Sunny" : "Cloudy",
                    precipitationChance = i * 10
                });
            }

            return Results.Ok(forecast);
        })
        .WithName("GetForecast");
    }
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary, string? city)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

