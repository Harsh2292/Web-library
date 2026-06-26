using Microsoft.Extensions.AI;
using OpenAI;                  
using Serilog;
using Serilog.Events;
using System.ClientModel;
using WebLibrary.AgenticApi.Agents;
using WebLibrary.AgenticApi.Middleware;
using WebLibrary.AgenticApi.Services;
using WebLibrary.AgenticApi.Workflows;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/WebLirary-agent-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IBlobFetcherService, BlobFetcherService>();
builder.Services.AddScoped<IPdfExtractorService, PdfExtractorService>();
builder.Services.AddScoped<IVectorDbService, VectorDbService>();
builder.Services.AddScoped<IGuardrailService, GuardrailService>();
builder.Services.AddScoped<IMetadataExtractorAgent, MetadataExtractorAgent>();
builder.Services.AddScoped<IBookMetadataWorkflow, BookMetadataWorkflow>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebLibraryPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var geminiApiKey = builder.Configuration["Gemini:ApiKey"]
    ?? throw new InvalidOperationException("Gemini:ApiKey is not configured");

builder.Services.AddSingleton<IChatClient>(_ =>
{
    var chatClient = new OpenAI.Chat.ChatClient(
        model: "gemini-2.0-flash",
        credential: new ApiKeyCredential(geminiApiKey),
        options: new OpenAI.OpenAIClientOptions
        {
            Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")
        });

    return chatClient.AsIChatClient();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalErrorMiddleware>();
app.UseHttpsRedirection();
app.UseCors("WebLibraryPolicy");
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("WebLibrary.AgenticApi starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WebLibrary.AgenticApi failed to start");
}
finally
{
    Log.CloseAndFlush();
}
