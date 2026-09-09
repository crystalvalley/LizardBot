using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using LizardBot.Discord;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new DiscordSocketClient(
    new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.Guilds
    }));

builder.Services.AddSingleton<InteractionService>(services =>
{
    var client = services.GetRequiredService<DiscordSocketClient>();
    return new InteractionService(client.Rest);
});

builder.Services.AddHostedService<DiscordBotService>();

var app = builder.Build();
app.Run();