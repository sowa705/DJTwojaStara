using System.Threading.Tasks;
using DJTwojaStara.Audio;
using DSharpPlus.Entities;

namespace DJTwojaStara.Services;

public interface IPlaybackService
{
    Task<PlaybackSession> CreateSession(DiscordChannel channel);
    PlaybackSession GetPlaybackSession(ulong channelId);
    PlaybackSession GetPlaybackSession(string sessionId);
    bool SessionExists(ulong channelId);
    bool SessionExists(string sessionId);
    int GetSessionCount();
}