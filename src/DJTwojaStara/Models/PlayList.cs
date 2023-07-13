using System.Collections.Generic;
using DJTwojaStara.Services;

namespace DJTwojaStara.Models;

public record PlayList
{
    public List<SongInfo> Songs { get; init; } = new();
    public List<string> SongOrder { get; init; } = new();
    
    public int CurrentSong { get; set; }
    public double CurrentPosition { get; set; }
}