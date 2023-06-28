using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DJTwojaStara.Audio;
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
    
    public async Task Respond(InteractionContext ctx, string message)
    {
        // create an embed
        var embed = new DiscordEmbedBuilder
        {
            Title = "DJTwojaStara",
            Description = message,
            Color = new DiscordColor("#20FF20"),
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
            var streamList = await _streamerService.StreamSongs(query);
            
            if (!streamList.Any())
            {
                await Respond(ctx, "No songs found");
                return;
            }
            
            session.AddToQueue(streamList);

            if (streamList.Count() == 1)
            {
                await streamList.First().DownloadMetadataAsync(); // Download metadata before sending the message
                await Respond(ctx, $"Added *{streamList.First().Name}* to the queue");
            }
            else
            {
                await Respond(ctx, $"Added {streamList.Count()} songs to the queue");
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
            
            var currentSong = session.GetQueue().FirstOrDefault();

            if (currentSong == null)
            {
                await Respond(ctx, "Nothing to skip");
                return;
            }
            
            session.Skip();
            
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
            
            var queue = session.GetQueue();
            var currentSong = queue.FirstOrDefault();
            
            var stringbuilder = new System.Text.StringBuilder();
            stringbuilder.AppendLine($"**Current song:** {currentSong?.Name ?? "Nothing"}");
            if (queue.Count() > 1)
            {
                stringbuilder.AppendLine("**Up next:**");
                foreach (var song in queue.Skip(1).Take(15))
                {
                    stringbuilder.AppendLine($"- {song.Name}");
                }

                if (queue.Count() > 16)
                {
                    stringbuilder.AppendLine($"... and {queue.Count() - 16} more");
                }
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
            var streams = await _streamerService.StreamSongs(query);
            var stream = streams.FirstOrDefault();
            await stream.DownloadMetadataAsync();
            
            session.SetInterruption(stream);
            
            await Respond(ctx, $"Interrupting playback with *{stream.Name}*");
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
    [SlashCommand("twojastara", "Twoja stara")]
    public async Task TwojaStaraAsync(InteractionContext ctx)
    {
        await Defer(ctx);
        try
        {
            // enqueue a random joke from the folder
            // get the directory of the executable
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var jokes = Directory.GetFiles(dir+"/twojastara");
            var joke = jokes[new Random().Next(jokes.Length)];
            // create an opusstreamable from the file
            var stream = new OpusFileStreamable(joke, "twojastara");
            // interrupt playback with the stream
            var channel = GetUserChannel(ctx);
            var session = _playbackService.GetPlaybackSession(channel.Id);
            session.SetInterruption(stream);
            
            await Respond(ctx, $"Twoja stara jest tak stara że jest stara");
        }
        catch (Exception e)
        {
            await RespondError(ctx, e);
        }
    }
}

