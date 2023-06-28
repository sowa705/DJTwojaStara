using System.Collections.Generic;
using System.Threading.Tasks;
using DJTwojaStara.Models;
using DJTwojaStara.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DJTwojaStara.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly PerformanceSnapshotRepository _performanceSnapshotRepository;
    
    public InfoController(PerformanceSnapshotRepository performanceSnapshotRepository)
    {
        _performanceSnapshotRepository = performanceSnapshotRepository;
    }
    
    [HttpGet]
    [Route("performance")]
    public async Task<ActionResult<IEnumerable<PerformanceSnapshot>>> GetPerformanceTable()
    {
        var snapshots = await _performanceSnapshotRepository.GetLastHourAsync(HttpContext.RequestAborted);
        return Ok(snapshots);
    }
}