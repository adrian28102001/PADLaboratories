using Microsoft.AspNetCore.Mvc;

namespace ApplicationManagementService.Controllers;


[Route("applicationmanagement/test/reroute")]
[ApiController]
public class TestController: ControllerBase
{
    
    [HttpGet]
    public async Task<ActionResult> Reroute()
    {
        return StatusCode(302);
    }
}