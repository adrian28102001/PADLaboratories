using JobManagementService.Entities;
using JobManagementService.Metric;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JobManagementService.Controllers;

[Route("jobmanagement/api/jobs/applications")]
public class ApplicationController : ControllerBase
{
    private readonly HttpClient _apiGatewayClient;
    private readonly ILogger<ApplicationController> _logger;

    public ApplicationController(IHttpClientFactory clientFactory, ILogger<ApplicationController> logger)
    {
        _logger = logger;
        _apiGatewayClient = clientFactory.CreateClient("APIGateway");
    }

    [HttpGet("{jobId}")]
    public async Task<OkObjectResult> GetJobApplications(int jobId)
    {
        MetricsRegistry.JobApplicationsGetCounter.Inc();

        try
        {
            Console.WriteLine("GET /GetJobApplications endpoint hit");
            var response = await _apiGatewayClient.GetAsync($"applicationmanagement/api/applications/job/{jobId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var application = JsonConvert.DeserializeObject<Application>(jsonResponse);
                return Ok(application);
            }
        }
        catch (Exception e)
        {
            return Ok($"Something went wrong while fetching data. Error: {e.Message}");
        }

        return Ok(null);
    }
}