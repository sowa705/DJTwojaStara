﻿using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using Nefarius.DSharpPlus.SlashCommands.Extensions.Hosting.Attributes;
using Nefarius.DSharpPlus.SlashCommands.Extensions.Hosting.Events;

namespace Template.Modules;

public class GeneralModule : ApplicationCommandModule
{
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
        var files = Directory.GetFiles(Path.GetTempPath() + "/botus-temp", "*", enumOpts);
        var size = files.Select(x => new FileInfo(x).Length).Sum();
        
        var embed = new DiscordEmbedBuilder
        {
            Title = "Server info",
            Description = $"*Bot info*\nVersion 0.2\n*Cache info*\n{files.Length} files\n{(size/1024.0/1024.0).ToString("0.0")} MB used temp space",
            Color = DiscordColor.Blurple
        };
        
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
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