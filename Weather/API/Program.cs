using API.Endpoints;
using API.Services;
using API.Models;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Configurar OpenAI Settings
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAISettings"));

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

builder.Services.AddHttpClient<IUtpcService, UtpcService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7247/");
});
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

app.Run();
