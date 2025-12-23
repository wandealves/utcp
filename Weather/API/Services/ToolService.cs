using OpenAI.Chat;

namespace API.Services;

public static class ToolService
{
    public static ChatTool[] GetTools()
    {
        return new[]
        {
            ChatTool.CreateFunctionTool(
                functionName: "get_weather",
                functionDescription: "Obtém o clima atual de uma cidade",
                functionParameters: BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new
                    {
                        city = new
                        {
                            type = "string",
                            description = "Nome da cidade"
                        }
                    },
                    required = new[] { "city" }
                })
            )
        };
    }
}
