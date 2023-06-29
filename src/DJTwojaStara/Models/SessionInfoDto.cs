using System.Collections.Generic;

namespace DJTwojaStara.Models;

public record SessionInfoDto
{
    public string Id { get; init; }
    public ulong ChannelId { get; init; }
    public SessionTrackDto CurrentTrack { get; init; }
    public List<SessionTrackDto> Queue { get; init; }
}

public record SessionTrackDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public float Length { get; init; }
    public string CoverUrl { get; init; }
}