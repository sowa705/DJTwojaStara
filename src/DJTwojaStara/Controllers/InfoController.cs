using System.Collections.Generic;
using System.Threading.Tasks;
using DJTwojaStara.Models;
using DJTwojaStara.Repositories;
using DJTwojaStara.Services;
using Microsoft.AspNetCore.Mvc;

namespace DJTwojaStara.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly PerformanceSnapshotRepository _performanceSnapshotRepository;
    private readonly IPlaybackService _playbackService;
    
    public InfoController(PerformanceSnapshotRepository performanceSnapshotRepository, IPlaybackService playbackService)
    {
        _performanceSnapshotRepository = performanceSnapshotRepository;
        _playbackService = playbackService;
    }
    
    [HttpGet]
    [Route("performance")]
    public async Task<ActionResult<IEnumerable<PerformanceSnapshot>>> GetPerformanceTable()
    {
        var snapshots = await _performanceSnapshotRepository.GetLastHourAsync(HttpContext.RequestAborted);
        return Ok(snapshots);
    }
    
    [HttpGet]
    [Route("sessioncount")]
    public Task<ActionResult<int>> GetSessionCount()
    {
        var count = _playbackService.GetSessionCount();
        return Task.FromResult<ActionResult<int>>(Ok(count));
    }
}