using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DJTwojaStara.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DJTwojaStara.Commands;

public class MusicCommands : ApplicationCommandModule
{
    private readonly ILogger _logger;
    private readonly YoutubeService _streamerService;
    private readonly IPlaybackService _playbackService;
    public MusicCommands(YoutubeService streamerService, ILogger<MusicCommands> logger, IPlaybackService playbackService)
    {
        _logger = logger;
        _streamerService = streamerService;
        _playbackService = playbackService;
    }
    
    [SlashCommand("play", "Play a song")]
    public async Task PlayAsync(InteractionContext ctx, [Option("Query", "Song title or link")] string query)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var embedBuilder = new DiscordEmbedBuilder()
        {
            Color = DiscordColor.Green
        };

        var response = await PlayHandler(ctx, query);

        embedBuilder.Description = response;
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
    }

    async Task<string> PlayHandler(InteractionContext ctx, string query)
    {
        var channel = ctx.Member.VoiceState?.Channel;
        
        if (channel == null)
        {
            return "User must be in a voice channel";
        }
        
        var streams = await _streamerService.StreamSongs(query);
        
        if (_playbackService.SessionExists(channel.Id))
        {
            string response;
            if (streams.Count() > 1)
            {
                response = $"<:botus:1076975001183469578> Enqueued {streams.Count()} songs";
            }
            else
            {
                var stream = streams.First();
                await stream.DownloadMetadataAsync();
                response = $"<:botus:1076975001183469578> Enqueued *{stream.Name}*";
            }
            _playbackService.GetPlaybackSession(channel.Id).AddToQueue(streams);
            return response;
        }
        else
        {
            string response;
            if (streams.Count() > 1)
            {
                response = $"<:botus:1076975001183469578> Creating new session and playing {streams.Count()} songs...";
            }
            else
            {
                var stream = streams.First();
                await stream.DownloadMetadataAsync();
                response = $"<:botus:1076975001183469578> Creating new session and playing *{stream.Name}* ...";
            }
            (await _playbackService.CreateSession(channel)).AddToQueue(streams);
            return response;
        }
    }
    
    /*
    
    [SlashCommand("skip", "Skip the currently playing song")]
    public async Task SkipAsync(InteractionContext ctx)
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null) { await RespondAsync("User must be in a voice channel."); return; }

        if (_playbackService.SessionExists(channel.Id))
        {
            var session = _playbackService.GetPlaybackSession(channel.Id);
            var queue = session.GetQueue();
            if (queue.Count == 0)
            {
                await RespondAsync($"<:botus:1076975001183469578> Empty queue. Ending *{queue[0].Name}*");
                session.Skip();
            }
            else
            {
                await RespondAsync($"<:botus:1076975001183469578> Skipping *{queue[0].Name}*");
                session.Skip();
            }
        }
        else
        {
            await RespondAsync($"<:botus:1076975001183469578> Empty queue and not playing anything. use `/play [search query]` to add a song.");
        }
    }
    
    [SlashCommand("queue", "Show the current queue")]
    public async Task QueueAsync()
    {
        await DeferAsync();
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null) { await FollowupAsync("User must be in a voice channel."); return; }

        if (_playbackService.SessionExists(channel.Id))
        {
            var session = _playbackService.GetPlaybackSession(channel.Id);
            var queue = session.GetQueue();
            await queue.First().DownloadMetadataAsync();
            if (queue.Count == 0)
            {
                await FollowupAsync($"Empty queue. use `/play [search query]` to add a song.");
                return;
            }
            var returnValue=$"<:botus:1076975001183469578> Currently playing: *{queue[0].Name}*";

            if (queue.Count > 1)
            {
                returnValue += "\n**Queue**";
            }

            int displayValues = queue.Count;
            if (displayValues > 6)
            {
                displayValues = 6;
            }

            for (int i = 1; i < displayValues; i++)
            {
                await queue[i].DownloadMetadataAsync();
                returnValue += $"\n{i}.\t{queue[i].Name}";
            }

            if (queue.Count>6)
            {
                returnValue += $"\n{queue.Count - 5} more...";
            }
            await FollowupAsync(returnValue);
        }
        else
        {
            await FollowupAsync($"<:botus:1076975001183469578> Empty queue. use `/play [search query]` to add a song.");
        }
    }
    
    [SlashCommand("eq", "Set the equalizer preset")]
    public async Task EQAsync([Summary("preset","EQ preset to use")]EQPreset preset)
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null) { await RespondAsync("User must be in a voice channel."); return; }

        if (_playbackService.SessionExists(channel.Id))
        {
            var session = _playbackService.GetPlaybackSession(channel.Id);
            session.SetEQPreset(preset);
            await RespondAsync($"<:botus:1076975001183469578> EQ preset set to {preset}");
        }
        else
        {
            await RespondAsync($"<:botus:1076975001183469578> Empty queue and not playing anything. use `/play [search query]` to add a song.");
        }
    }
    
    [SlashCommand("interrupt", "Interrupt playback with a specific song")]
    public async Task InterruptAsync([Summary("Query", "Song title or link")] string query)
    {
        await DeferAsync();
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null) { await FollowupAsync("User must be in a voice channel."); return; }

        if (_playbackService.SessionExists(channel.Id))
        {
            var session = _playbackService.GetPlaybackSession(channel.Id);
            var streams = await _streamerService.StreamSongs(query);
            var stream = streams.FirstOrDefault();
            await stream.DownloadMetadataAsync();
            
            session.SetInterruption(stream);
            
            await FollowupAsync($"<:botus:1076975001183469578> Interrupted playback with {stream.Name}");
        }
        else
        {
            await FollowupAsync($"<:botus:1076975001183469578> Session you are trying to interrupt does not exist. Use `/play [search query]` to add a song.");
        }
    }
    
    [SlashCommand("disconnect", "Disconnects from the voice channel")]
    public async Task DisconnectAsync()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null) { await RespondAsync("User must be in a voice channel."); return; }

        if (_playbackService.SessionExists(channel.Id))
        {
            var session = _playbackService.GetPlaybackSession(channel.Id);
            await session.DisconnectAsync();
            await RespondAsync($"<:botus:1076975001183469578> Disconnected from voice channel.");
        }
        else
        {
            await RespondAsync($"<:botus:1076975001183469578> Not connected to voice channel. Use `/play [search query]` to add a song.");
        }
    }
    
    */
}

