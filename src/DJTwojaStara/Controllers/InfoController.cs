using System.Collections.Generic;
using System.Threading.Tasks;
using DJTwojaStara.Models;
using DJTwojaStara.Repositories;
using DJTwojaStara.Services;
using Microsoft.AspNetCore.Mvc;
using Nefarius.DSharpPlus.Extensions.Hosting;

namespace DJTwojaStara.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly PerformanceSnapshotRepository _performanceSnapshotRepository;
    private readonly IPlaybackService _playbackService;
    private readonly IDiscordClientService _discordClientService;
    
    public InfoController(PerformanceSnapshotRepository performanceSnapshotRepository, IPlaybackService playbackService, IDiscordClientService discordClientService)
    {
        _performanceSnapshotRepository = performanceSnapshotRepository;
        _playbackService = playbackService;
        _discordClientService = discordClientService;
    }
    
    [HttpGet]
    [Route("performance")]
    public async Task<ActionResult<IEnumerable<PerformanceSnapshot>>> GetPerformanceTable()
    {
        var snapshots = await _performanceSnapshotRepository.GetLastHourAsync(HttpContext.RequestAborted);
        return Ok(snapshots);
    }
    
    [HttpGet]
    [Route("version")]
    public async Task<ActionResult<IEnumerable<PerformanceSnapshot>>> GetApplicationVersion()
    {
        var version = typeof(Program).Assembly.GetName().Version;
        return Ok(version);
    }
    
    [HttpGet]
    [Route("sessioncount")]
    public Task<ActionResult<int>> GetSessionCount()
    {
        var count = _playbackService.GetSessionCount();
        return Task.FromResult<ActionResult<int>>(Ok(count));
    }
    [HttpGet]
    [Route("invite/enabled")]
    public Task<ActionResult<bool>> GetInviteEnablement()
    {
        return Task.FromResult<ActionResult<bool>>(Ok(true));
    }
    
    [HttpGet]
    [Route("invite/link")]
    public Task<ActionResult<string>> GetInviteLink()
    {
        var url = $"https://discord.com/api/oauth2/authorize?client_id={_discordClientService.Client.CurrentApplication.Id}&permissions=2150647808&scope=bot%20applications.commands";
        
        return Task.FromResult<ActionResult<string>>(Ok(url));
    }
}