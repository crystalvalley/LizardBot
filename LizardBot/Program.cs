using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LizardBot.Configuration;
using LizardBot.Discord;
using LizardBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Register the Discord gateway client as a singleton.
// LizardBot only uses slash commands, so the Guilds intent is enough for now.
// Additional intents can be enabled later if message or member events are needed.
builder.Services.AddSingleton(new DiscordSocketClient(
    new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.Guilds
    }));

// InteractionService discovers and executes Discord interaction modules
// such as slash commands.
builder.Services.AddSingleton<InteractionService>(services =>
{
    var client = services.GetRequiredService<DiscordSocketClient>();

    return new InteractionService(client.Rest);
});

// Runs the Discord bot connection as a background service
// for the entire lifetime of the ASP.NET Core application.
builder.Services.AddHostedService<DiscordBotService>();

// Bind managed server definitions from the "ServerManagement"
// section in appsettings.json.
//
// By default, ASP.NET Core watches appsettings.json for changes and reloads
// the configuration automatically. Services using IOptionsMonitor<T>
// can access the updated values while LizardBot is still running,
// so adding or changing a server does not require restarting the application.
builder.Services.Configure<ServerManagementOptions>(
    builder.Configuration.GetSection("ServerManagement"));

// Provides server and service availability checks used by Discord commands
// and, later, by the background monitoring service.
builder.Services.AddSingleton<ServerStatusService>();

// Sends Wake-on-LAN Magic Packets to configured servers.
builder.Services.AddSingleton<WakeOnLanService>();

var app = builder.Build();

// Simple endpoint that confirms the ASP.NET Core host itself is running.
// This does not represent the status of any managed server.
app.MapGet("/", () => "LizardBot is running.");

// Lightweight health endpoint for external monitoring or deployment checks.
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "LizardBot"
}));

app.Run();