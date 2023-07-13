using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DJTwojaStara.Audio;
using DJTwojaStara.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DJTwojaStara.Commands;

public class MusicCommands : ApplicationCommandModule
{
    private readonly ILogger _logger;
    private readonly string _websiteUrl;
    private readonly IStreamerService _streamerService;
    private readonly ISearchService _searchService;
    private readonly IPlaybackService _playbackService;
    public MusicCommands(IStreamerService streamerService, ILogger<MusicCommands> logger, IPlaybackService playbackService, ISearchService searchService, IConfiguration configuration)
    {
        _logger = logger;
        _streamerService = streamerService;
        _searchService = searchService;
        _playbackService = playbackService;
        _websiteUrl = configuration["WebUIUrl"];
    }
    
    public DiscordChannel GetUserChannel(InteractionContext ctx)
    {
        var channel = ctx.Member.VoiceState.Channel;
        if (channel == null)
        {
            throw new Exception("User must be in a voice channel.");
        }
        return channel;
    }
    
    public async Task Defer(InteractionContext ctx)
    {
        try
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to defer response");
        }
    }
    
    public async Task Respond(InteractionContext ctx, string message, string? id = null)
    {
        // create an embed
        var embed = new DiscordEmbedBuilder
        {
            Title = "DJTwojaStara",
            Description = message,
            Color = new DiscordColor("#20FF20"),
            Url = id != null ? _websiteUrl + "/player/" + id : null,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Playing garbage since 2023",
                IconUrl = "https://cdn.discordapp.com/emojis/1076975001183469578.png?v=1"
            }
        };
        
        // send the embed
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
    
    public async Task RespondError(InteractionContext ctx, Exception e)
    {
        // create an embed
        var embed = new DiscordEmbedBuilder
        {
            Title = "Error",
            Description = $"{e.Message}\n{e.StackTrace}",
            Color = new DiscordColor("#FF2020"),
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Playing garbage since 2023",
                IconUrl = "https://cdn.discordapp.com/emojis/1076975001183469578.png?v=1"
            }
        };
        
        // send the embed
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
    
    [SlashCommand("play", "Play a song")]
    public async Task PlayAsync(InteractionContext ctx, [Option("Query", "Song title or link")] string query)
    {
        await Defer(ctx);

        try
        {
            var channel = GetUserChannel(ctx);

            if (!_playbackService.SessionExists(channel.Id))
            {
                await _playbackService.CreateSession(channel);
            }
            
            var session = _playbackService.GetPlaybackSession(channel.Id);
            var searchList = await _searchService.Search(query);
            if (!searchList.Any())
            {
                await Respond(ctx, "No songs found",session.id);
                return;
            }
            
            await session.AddToQueue(searchList);

            if (searchList.Count() == 1)
            {
                await Respond(ctx, $"Added *{searchList.First().Name}* to the queue",session.id);
            }
            else
            {
                await Respond(ctx, $"Added {searchList.Count()} songs to the queue",session.id);
            }
        }
        catch (Exception e)
        {
            await RespondError(ctx, e);
        }
    }
    [SlashCommand("skip", "Skip the currently playing song")]
    public async Task SkipAsync(InteractionContext ctx)
    {
        await Defer(ctx);
        
        try
        {
            var channel = GetUserChannel(ctx);
            var session = _playbackService.GetPlaybackSession(channel.Id);
            
            var currentSong = session.PlayList.Songs[session.PlayList.CurrentSong];

            if (currentSong == null)
            {
                await Respond(ctx, "Nothing to skip");
                return;
            }
            
            session.NextSong();
            
            await Respond(ctx, $"Skipping *{currentSong.Name}*");
        }
        catch (Exception e)
        {
            await RespondError(ctx, e);
        }
    }
    [SlashCommand("queue", "Show the current queue")]
    public async Task QueueAsync(InteractionContext ctx)
    {
        await Defer(ctx);
        try
        {
            var channel = GetUserChannel(ctx);
            var session = _playbackService.GetPlaybackSession(channel.Id);
            
            var queue = session.PlayList.Songs.Skip(session.PlayList.CurrentSong + 1).Take(10).ToList();
            
            var stringbuilder = new StringBuilder();
            stringbuilder.AppendLine($"**Now playing:** {session.PlayList.Songs[session.PlayList.CurrentSong].Name}");
            stringbuilder.AppendLine();
            stringbuilder.AppendLine("**Up next:**");
            foreach (var song in queue)
            {
                stringbuilder.AppendLine($"- {song.Name}");
            }
            
            await Respond(ctx, stringbuilder.ToString());
        }
        catch (Exception e)
        {
            await RespondError(ctx, e);
        }
    }
    [SlashCommand("eq", "Set the equalizer preset")]
    public async Task EQAsync(InteractionContext ctx ,[Option("preset","EQ preset to use")] EQPreset preset)
    {
        await Defer(ctx);
        try
        {
            var channel = GetUserChannel(ctx);
            var session = _playbackService.GetPlaybackSession(channel.Id);
            
            session.SetEQPreset(preset);
            
            await Respond(ctx, $"Set EQ preset to *{preset}*");
        }
        catch (Exception e)
        {
            await RespondError(ctx, e);
        }
    }
    [SlashCommand("interrupt", "Interrupt playback with a specific song")]
    public async Task InterruptAsync(InteractionContext ctx, [Option("Query", "Song title or link")] string query)
    {
        await Defer(ctx);
        try
        {
            var channel = GetUserChannel(ctx);
            
            var session = _playbackService.GetPlaybackSession(channel.Id);
            var searchList = await _searchService.Search(query);
            
            if (!searchList.Any())
            {
                await Respond(ctx, "No songs found");
                return;
            }
            
            var stream = await _streamerService.GetStreamable(searchList.First());
            
            session.SetInterruption(stream);
            
            await Respond(ctx, $"Interrupting playback with *{searchList.First().Name}*");
        }
        catch (Exception e)
        {
            await RespondError(ctx, e);
        }
    }
    [SlashCommand("disconnect", "Disconnects from the voice channel")]
    public async Task DisconnectAsync(InteractionContext ctx)
    {
        await Defer(ctx);
        try
        {
            var channel = GetUserChannel(ctx);
            
            await _playbackService.GetPlaybackSession(channel.Id).DisconnectAsync();
            await Respond(ctx, "Disconnected from voice channel.");
        }
        catch (Exception e)
        {
            await RespondError(ctx, e);
        }
    }
    
    [SlashCommand("id", "Gets the ID of the session")]
    public async Task IdAsync(InteractionContext ctx)
    {
        await Defer(ctx);
        try
        {
            var channel = GetUserChannel(ctx);
            var session = _playbackService.GetPlaybackSession(channel.Id);
            
            await Respond(ctx, $"Session ID: {session.id}");
        }
        catch (Exception e)
        {
            await RespondError(ctx, e);
        }
    }
}

