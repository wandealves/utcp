using OpenAI.Chat;
using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using API.Models;

namespace API.Services;

public interface IChatService
{
    Task<string> SendMessageAsync(string userMessage);
}

public class ChatService : IChatService
{
    private readonly ChatClient _chat;
    private readonly IUtpcService _utpcService;

    public ChatService(IUtpcService utpcService, IOptions<OpenAISettings> settings)
    {
        _utpcService = utpcService;
        _chat = new ChatClient(
            model: settings.Value.Model,
            apiKey: settings.Value.ApiKey
        );
    }

    public async Task<string> SendMessageAsync(string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("Você é um assistente que usa ferramentas quando necessário."),
            new UserChatMessage(userMessage)
        };

        var options = new ChatCompletionOptions();
        foreach (var tool in ToolService.GetTools())
        {
            options.Tools.Add(tool);
        }
        var response = await _chat.CompleteChatAsync(messages, options);
        var assistantMessage = response.Value;
        if (assistantMessage.ToolCalls.Count > 0)
        {
            var toolCall = assistantMessage.ToolCalls[0];
            var argsJson = JsonSerializer.Deserialize<JsonElement>(toolCall.FunctionArguments.ToString());
            var toolResult = await _utpcService.ExecuteAsync(
                toolCall.FunctionName,
                argsJson
            );
            messages.Add(new AssistantChatMessage(assistantMessage));
            messages.Add(new ToolChatMessage(
                toolCall.Id,
                JsonSerializer.Serialize(toolResult)
            ));
            var finalResponse = await _chat.CompleteChatAsync(messages, options);
            return finalResponse.Value.Content[0].Text;
        }
        return assistantMessage.Content[0].Text;
    }
}
