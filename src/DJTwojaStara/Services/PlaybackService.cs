using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DJTwojaStara.Audio;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;

namespace DJTwojaStara.Services;
public class PlaybackService: IPlaybackService
{
    private ILogger<PlaybackService> _logger;
    private readonly IStreamerService _streamerService;
    public List<PlaybackSession> Sessions = new List<PlaybackSession>();

    public PlaybackService(ILogger<PlaybackService> logger, IStreamerService streamerService)
    {
        _logger = logger;
        _streamerService = streamerService;
    }

    public async Task<PlaybackSession> CreateSession(DiscordChannel channel)
    {
        var audioClient = await channel.ConnectAsync();

        var session = new PlaybackSession(channel.Id, audioClient, _logger, _streamerService);
        Sessions.Add(session);

        Task.Run(() => session.StartStreamAsync()); // Start in the background without awaiting
        
        return session;
    }

    public PlaybackSession GetPlaybackSession(ulong channelId)
    {
        Sessions.RemoveAll(x => x.Disconnected);
        if (!SessionExists(channelId))
        {
            throw new KeyNotFoundException("Session does not exist");
        }
        return Sessions.FirstOrDefault(x => x.ChannelID == channelId);
    }

    public PlaybackSession GetPlaybackSession(string sessionId)
    {
        Sessions.RemoveAll(x => x.Disconnected);
        if (!SessionExists(sessionId))
        {
            throw new KeyNotFoundException("Session does not exist");
        }
        return Sessions.FirstOrDefault(x => x.id == sessionId);
    }

    public bool SessionExists(ulong channelId)
    {
        Sessions.RemoveAll(x => x.Disconnected);
        return Sessions.Any(x => x.ChannelID == channelId);
    }

    public bool SessionExists(string sessionId)
    {
        Sessions.RemoveAll(x => x.Disconnected);
        return Sessions.Any(x => x.id == sessionId);
    }

    public int GetSessionCount()
    {
        Sessions.RemoveAll(x => x.Disconnected);
        return Sessions.Count;
    }
}