using Microsoft.AspNetCore.Mvc;

namespace ApplicationManagementService.Controllers;

[Route("applicationmanagement/test")]
[ApiController]
public class TestController : ControllerBase
{
    // This counter simulates a persistent state where we can count the number of times
    // the reroute endpoint has been called.
    private static int _callCount = 0;

    [HttpGet("reroute")]
    public ActionResult Reroute()
    {
        _callCount++;
        
        // Here, we simulate a failure that should trip the circuit breaker
        // after a certain number of calls.
        // We will return a 500 Internal Server Error to simulate the service failing.
        if (_callCount < 5)
        {
            Console.WriteLine("Simulating service failure. Returning 500 status code.");
            return StatusCode(500); // Simulate service failure
        }

        Console.WriteLine("Service has recovered. Returning 200 status code.");
        // After 5 calls, we simulate the service recovering,
        // and start returning a 200 OK to reset the circuit breaker.
        return Ok("Service has recovered");
    }
}
