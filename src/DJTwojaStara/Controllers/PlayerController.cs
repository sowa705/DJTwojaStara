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
    private readonly IStreamerService _streamerService;
    
    public PlayerController(IPlaybackService playbackService, IStreamerService streamerService)
    {
        _playbackService = playbackService;
        _streamerService = streamerService;
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
            CurrentTrack = new SessionTrackDto()
            {
                Name = session.GetQueue().First().Name,
                Length = 0,
                CoverUrl = session.GetQueue().First().CoverUrl
            },
            Queue = session.GetQueue().Skip(1).Select(x=>new SessionTrackDto()
            {
                Name = x.Name,
                Length = 0,
                CoverUrl = x.CoverUrl
            }).ToList()
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
        
        var streamList = await _streamerService.StreamSongs(query);

        var streamables = streamList as IStreamable[] ?? streamList.ToArray();

        if (!streamables.Any())
        {
            return BadRequest("No songs found");
        }
            
        session.AddToQueue(streamables);

        if (streamables.Count() == 1)
        {
            await streamables.First().DownloadMetadataAsync(); // Download metadata before sending the message
            return Ok();
        }

        return Ok();
    }
}