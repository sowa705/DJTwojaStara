using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DJTwojaStara.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        return Ok();
    }
}