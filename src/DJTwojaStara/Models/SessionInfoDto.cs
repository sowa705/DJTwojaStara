using System.Collections.Generic;
using DJTwojaStara.Audio;
using DJTwojaStara.Services;

namespace DJTwojaStara.Models;

public record SessionInfoDto
{
    public string Id { get; init; }
    public ulong ChannelId { get; init; }
    
    public string EqPreset { get; init; }
    
    public PlayList PlayList { get; init; }
}