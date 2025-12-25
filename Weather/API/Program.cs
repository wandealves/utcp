using API.Endpoints;
using API.Models;
using API.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Configurar OpenAI Settings
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));

builder.Services.Configure<UTCPSettings>(
    builder.Configuration.GetSection("UTCP"));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.SetReferenceHostDocument();

        return Task.CompletedTask;
    });
});

builder.Services.AddScoped<IUtcpService, UtcpService>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.RegistrarChatEndpoints();
app.RegistrarWeatherEndpoints();
app.RegistrarDiscoveryEndpoints();

app.Run();
