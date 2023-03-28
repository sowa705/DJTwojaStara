using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DJTwojaStara.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using Nefarius.DSharpPlus.SlashCommands.Extensions.Hosting.Attributes;
using Nefarius.DSharpPlus.SlashCommands.Extensions.Hosting.Events;

namespace Template.Modules;

public class GeneralModule : ApplicationCommandModule
{
    private readonly IAiService _aiService;
    
    public GeneralModule(IAiService aiService)
    {
        _aiService = aiService;
    }
    
    // Declare application command.
    [SlashCommand("latency", "Display bot latency")]
    public async Task LatencyAsync(InteractionContext ctx)
    {
        int latency = ctx.Client.Ping;

        var embed = new DiscordEmbedBuilder
        {
            Title = "Pong!",
            Description = $"Latency: {ctx.Client.Ping} ms",
            Color = DiscordColor.Green
        };

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }
    
    // Declare application command.
    [SlashCommand("info", "Display bot info")]
    public async Task InfoAsync(InteractionContext ctx)
    {
        var enumOpts = new EnumerationOptions();
        enumOpts.RecurseSubdirectories = true;
        var files = Directory.GetFiles(Path.GetTempPath() + "/djtwojastara-temp", "*", enumOpts);
        var size = files.Select(x => new FileInfo(x).Length).Sum();
        
        var embed = new DiscordEmbedBuilder
        {
            Title = "DJTwojaStara",
            Description = $"*Bot info*\nVersion 0.3\n*Cache info*\n{files.Length} files\n{(size/1024.0/1024.0).ToString("0.0")} MB used temp space",
            Color = DiscordColor.Blurple
        };
        
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }
    
    [SlashCommand("clearcache", "Clear music cache")]
    public async Task ClearCacheAsync(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        var enumOpts = new EnumerationOptions();
        enumOpts.RecurseSubdirectories = true;
        var files = Directory.GetFiles(Path.GetTempPath() + "/djtwojastara-temp/ytdlp-cache", "*", enumOpts);
        
        foreach (var file in files)
        {
            File.Delete(file);
        }
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
        {
            Title = "DJTwojaStara",
            Description = $"Removed {files.Length} files from cache",
            Color = DiscordColor.Green
        }));
    }
    [SlashCommand("kill", "Kill the bot so it gets restarted")]
    public async Task KillAsync(InteractionContext ctx)
    {
        var embed = new DiscordEmbedBuilder
        {
            Title = "DJTwojaStara",
            Description = $"brb i gtg kms",
            Color = DiscordColor.Red
        };
        
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        
        Process.GetCurrentProcess().Kill();
    }
    
    [SlashCommand("ask", "Ask the bot a question")]
    public async Task AskAsync(InteractionContext ctx, [Option("question", "The question to ask")] string prompt)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        await _aiService.RespondToMessageAsync(prompt, ctx);
    }
}
[DiscordSlashCommandsEventsSubscriber]
public class SlashCommandsErrorHandler : IDiscordSlashCommandsEventsSubscriber
{
    private readonly ILogger<SlashCommandsErrorHandler> _logger;

    public SlashCommandsErrorHandler(ILogger<SlashCommandsErrorHandler> logger)
    {
        _logger = logger;
    }
    
    public Task SlashCommandsOnContextMenuErrored(SlashCommandsExtension sender, ContextMenuErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error while executing context menu");
        return Task.CompletedTask;
    }

    public Task SlashCommandsOnContextMenuExecuted(SlashCommandsExtension sender, ContextMenuExecutedEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task SlashCommandsOnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error while executing slash command");
        return Task.CompletedTask;
    }

    public Task SlashCommandsOnSlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs args)
    {
        return Task.CompletedTask;
    }
}