using System;
using System.Linq;
using System.Threading.Tasks;
using DJTwojaStara.Audio;
using DJTwojaStara.Models;
using DJTwojaStara.Services;
using Microsoft.AspNetCore.Mvc;

namespace DJTwojaStara.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IPlaybackService _playbackService;
    private readonly ISearchService _searchService;
    
    public PlayerController(IPlaybackService playbackService, ISearchService searchService)
    {
        _playbackService = playbackService;
        _searchService = searchService;
    }
    
    [HttpGet]
    [Route("{sessionID}")]
    public async Task<ActionResult<SessionInfoDto>> GetSessionInfo([FromRoute] string sessionId)
    {
        if (!_playbackService.SessionExists(sessionId))
        {
            return NotFound();
        }
        
        var session = _playbackService.GetPlaybackSession(sessionId);
        var dto = new SessionInfoDto
        {
            Id = session.id,
            ChannelId = session.ChannelID,
            EqPreset = Enum.GetName(typeof(EQPreset), session.EQPreset),
            PlayList = session.PlayList
        };
        
        return Ok(dto);
    }
    
    [HttpPost]
    [Route("{sessionID}/addtrack")]
    public async Task<ActionResult> AddTrackToSession([FromRoute] string sessionId, [FromBody] string query)
    {
        if (!_playbackService.SessionExists(sessionId))
        {
            return NotFound();
        }
        
        var session = _playbackService.GetPlaybackSession(sessionId);
        
        var searchList = await _searchService.Search(query);

        if (!searchList.Any())
        {
            return BadRequest("No songs found");
        }
        session.AddToQueue(searchList);

        return Ok();
    }
    
    [HttpPost]
    [Route("{sessionID}/skip/{trackId}")]
    public async Task<ActionResult> SkipTrack([FromRoute] string sessionId,[FromRoute] int trackId)
    {
        if (!_playbackService.SessionExists(sessionId))
        {
            return NotFound();
        }
        
        var session = _playbackService.GetPlaybackSession(sessionId);
        
        session.RemoveById(trackId);
        
        return Ok();
    }
    
    [HttpPost]
    [Route("{sessionID}/next")]
    public async Task<ActionResult> NextTrack([FromRoute] string sessionId)
    {
        if (!_playbackService.SessionExists(sessionId))
        {
            return NotFound();
        }
        
        var session = _playbackService.GetPlaybackSession(sessionId);
        
        session.NextSong();
        
        return Ok();
    }
    
    [HttpPost]
    [Route("{sessionID}/prev")]
    public async Task<ActionResult> PrevTrack([FromRoute] string sessionId)
    {
        if (!_playbackService.SessionExists(sessionId))
        {
            return NotFound();
        }
        
        var session = _playbackService.GetPlaybackSession(sessionId);
        
        session.PreviousSong();
        
        return Ok();
    }
    
    [HttpPost]
    [Route("{sessionID}/eq/{preset}")]
    public async Task<ActionResult> SetEq([FromRoute] string sessionId, [FromRoute] string preset)
    {
        if (!_playbackService.SessionExists(sessionId))
        {
            return NotFound();
        }
        
        var session = _playbackService.GetPlaybackSession(sessionId);

        if (!Enum.TryParse<EQPreset>(preset, out var presetenum))
        {
            return BadRequest("Invalid preset");
        }
        
        session.SetEQPreset(presetenum);
        
        return Ok();
    }
}