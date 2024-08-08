using System.Runtime.InteropServices;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LizardBot.Common.Exceptions;
using LizardBot.Common.Utils;
using LizardBot.Data;
using LizardBot.DiscordBot;
using LizardBot.DiscordBot.DiscordHandler;
using LizardBot.DiscordBot.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddLogging(builder => builder.AddConsole());

builder.Services.AddSingleton<LizardBotClient>(provider => new(
    new DiscordSocketConfig()
    {
        LogLevel = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? LogSeverity.Debug : LogSeverity.Info,
        AlwaysDownloadUsers = true,
        MessageCacheSize = 1024 * 1000,
        GatewayIntents = GatewayIntents.All,
    },
    provider.GetRequiredService<IConfiguration>()));

builder.Services.AddSingletonWithAsyncInit<LizardBotCommandService>(async provider =>
{
    var command = new LizardBotCommandService(
        new CommandServiceConfig()
        {
            DefaultRunMode = RunMode.Async,
            CaseSensitiveCommands = false,
            LogLevel = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? LogSeverity.Debug : LogSeverity.Info,
        }, provider);
    await command.InitAsync();
    return command;
});
var connectionString = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
        builder.Configuration.GetConnectionString("PsqlLocal")
        : builder.Configuration.GetConnectionString("PsqlPrd")) ?? throw new NoSettingDataException("ConnectionString");

builder.Services.AddDbContextFactory<LizardBotDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddHostedService<CommandHandler>();
builder.Services.AddHostedService<ChatBotHandler>();

builder.Services.AddRestClients(builder.Configuration.GetSection("ChatGPT"));

builder.Services.AddSingleton<GeneralService>();
builder.Services.AddSingleton<ChatBotService>();

await builder.Build().RunAsync();