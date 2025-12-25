using API.Models;

namespace API.Endpoints;
public static class DiscoveryEndpoint
{
    public static void RegistrarDiscoveryEndpoints(this IEndpointRouteBuilder routes)
    {
        var chat = routes.MapGroup("/api/v1/utcp")
            .WithName("Discovery")
            .WithTags("Discovery");

        chat.MapGet("/", () =>
        {
            var manual = new UtcpManual
            {
                ManualVersion = "1.0.0",
                UtcpVersion = "1.1.0",
                Name = "Weather API",
                Description = "API para informações e previsões meteorológicas",
                Tools = new List<Tool>
        {
        new Tool
            {
                Name = "get_current_weather",
                Description = "Obter o clima atual para uma localização específica",
                Tags = new[] { "clima", "atual", "temperatura" },
                Inputs = new ToolInputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, PropertySchema>
                    {
                        ["location"] = new PropertySchema
                        {
                            Type = "string",
                            Description = "Nome da cidade ou coordenadas (por exemplo, Londres ou 51.5074,-0.1278)"
                        },
                        ["units"] = new PropertySchema
                        {
                            Type = "string",
                            Description = "Unidades de temperatura",
                            Enum = new[] { "celsius", "fahrenheit" }
                        }
                    },
                    Required = new[] { "location" }
                },
                Output = new ToolOutputSchema
                {
                    Type = "object",
                    Description = "Informações do clima atual"
                },
                AverageResponseSize = 512,
                ToolCallTemplate = new HttpCallTemplate
                {
                    CallTemplateType = "http",
                    Url = "https://localhost:7247/api/v1/weatherforecast/api/v1/current",
                    HttpMethod = "GET",
                    QueryParams = new Dictionary<string, string>
                    {
                        ["location"] = "${location}",
                        ["units"] = "${units}"
                    }
                }
            },
            new Tool
            {
                Name = "get_forecast",
                Description = "Obter a previsão do tempo para os próximos dias",
                Tags = new[] { "clima", "previsão", "prognóstico" },
                Inputs = new ToolInputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, PropertySchema>
                    {
                        ["location"] = new PropertySchema
                        {
                            Type = "string",
                            Description = "Nome da cidade"
                        },
                        ["days"] = new PropertySchema
                        {
                            Type = "integer",
                            Description = "Número de dias (1-7)",
                            Minimum = 1,
                            Maximum = 7
                        }
                    },
                    Required = new[] { "location", "days" }
                },
                Output = new ToolOutputSchema
                {
                    Type = "array",
                    Description = "Array de previsões diárias"
                },
                AverageResponseSize = 2048,
                ToolCallTemplate = new HttpCallTemplate
                {
                    CallTemplateType = "http",
                    Url = "https://localhost:7247/api/v1/weatherforecast/api/v1/forecast",
                    HttpMethod = "GET",
                    QueryParams = new Dictionary<string, string>
                    {
                        ["location"] = "${location}",
                        ["days"] = "${days}"
                    }
                }
            }
        }
            };

            return Results.Ok(manual);
        })
        .WithName("Send Message UTCP");
    }
}

