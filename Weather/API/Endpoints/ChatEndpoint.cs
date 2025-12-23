using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class ChatEndpoint
{
    public static void RegistrarChatEndpoints(this IEndpointRouteBuilder routes)
    {
        var chat = routes.MapGroup("/api/v1/chats")
            .WithName("Chat")
            .WithTags("Chat");

        chat.MapPost("/", async ([FromBody] string userMessage, IChatService chatService) =>
        {
            try
            {
                var response = await chatService.SendMessageAsync(userMessage);
                return Results.Ok(new { response });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("Send Message");
    }
}
