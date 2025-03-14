using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Services;
using PoSnakeGame.Infrastructure.Configuration;
using PoSnakeGame.Infrastructure.Services;
using PoSnakeGame.Web.Components;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Create logs directory if it doesn't exist
var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
Directory.CreateDirectory(logPath);

// Configure logging with file provider
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFile(Path.Combine(logPath, "game-{Date}.log"));

// Configure Azure Table Storage
var tableConfig = new TableStorageConfig
{
    ConnectionString = builder.Configuration.GetConnectionString("AzureTableStorage") 
                      ?? "UseDevelopmentStorage=true", // Use local emulator if not configured
    HighScoresTableName = "HighScores",
    GameStatisticsTableName = "GameStatistics"
};
builder.Services.AddSingleton(tableConfig);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register game services
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<GameEngine>();
builder.Services.AddSingleton<TableStorageService>();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();