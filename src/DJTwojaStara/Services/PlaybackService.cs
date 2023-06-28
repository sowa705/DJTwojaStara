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
    public List<PlaybackSession> Sessions = new List<PlaybackSession>();

    public PlaybackService(ILogger<PlaybackService> logger)
    {
        _logger = logger;
    }

    public async Task<PlaybackSession> CreateSession(DiscordChannel channel)
    {
        
        var audioClient = await channel.ConnectAsync();
            
        var session = new PlaybackSession(channel.Id, audioClient, _logger);
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

    public bool SessionExists(ulong channelId)
    {
        Sessions.RemoveAll(x => x.Disconnected);
        return Sessions.Any(x => x.ChannelID == channelId);
    }
}