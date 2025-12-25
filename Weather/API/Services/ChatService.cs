using API.Models;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

namespace API.Services;

public interface IChatService
{
    Task<string> SendMessageAsync(string userMessage);
}

public class ChatService : IChatService
{
    private readonly ChatClient _chat;
    private readonly IUtcpService _utcpService;
    private readonly UTCPSettings _utcpSetting;

    public ChatService(IUtcpService utpcService, IOptions<OpenAISettings> settings, IOptions<UTCPSettings> utcpSetting)
    {
        _utcpService = utpcService;
        _chat = new ChatClient(
            model: settings.Value.Model,
            apiKey: settings.Value.ApiKey
        );
        _utcpSetting = utcpSetting.Value;
    }

    public async Task<string> SendMessageAsync(string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("Você é um assistente que usa ferramentas quando necessário."),
            new UserChatMessage(userMessage)
        };

        foreach (var manualUrl in _utcpSetting.ManualUrls)
        {
            try
            {
                await _utcpService.DiscoverToolsAsync(manualUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to discover tools: {ex.Message}");
                throw;
            }
        }

        var options = new ChatCompletionOptions();

        // Add UTCP tools discovered from the manual
        foreach (var tool in _utcpService.GetToolsForOpenAI())
        {
            options.Tools.Add(tool);
        }

        var response = await _chat.CompleteChatAsync(messages, options);
        var assistantMessage = response.Value;

        // Handle tool calls
        if (assistantMessage.ToolCalls.Count > 0)
        {
            var toolCall = assistantMessage.ToolCalls[0];

            // Parse arguments from OpenAI
            var argsJson = JsonSerializer.Deserialize<JsonElement>(
                toolCall.FunctionArguments.ToString()
            );

            // Execute UTCP tool
            var toolResult = await _utcpService.ExecuteAsync(
                toolCall.FunctionName,
                argsJson
            );

            // Add assistant message with tool call
            messages.Add(new AssistantChatMessage(assistantMessage));

            // Add tool result
            messages.Add(new ToolChatMessage(
                toolCall.Id,
                JsonSerializer.Serialize(toolResult)
            ));

            // Get final response from OpenAI
            var finalResponse = await _chat.CompleteChatAsync(messages, options);
            return finalResponse.Value.Content[0].Text;
        }

        return assistantMessage.Content[0].Text;
    }
}
