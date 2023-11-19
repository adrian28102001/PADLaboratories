using JobManagementService.Entities;
using JobManagementService.Metric;
using JobManagementService.Services.JobOffer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JobManagementService.Controllers;

[Route("jobmanagement/api/jobs/applications")]
public class ApplicationController : ControllerBase
{
    private readonly HttpClient _apiGatewayClient;
    private readonly IJobOfferService _jobOfferService;
    private readonly ILogger<ApplicationController> _logger;

    public ApplicationController(IHttpClientFactory clientFactory, ILogger<ApplicationController> logger, IJobOfferService jobOfferService)
    {
        _logger = logger;
        _jobOfferService = jobOfferService;
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
                var application = JsonConvert.DeserializeObject<IList<Application>>(jsonResponse);
                return Ok(application);
            }
        }
        catch (Exception e)
        {
            return Ok($"Something went wrong while fetching data. Error: {e.Message}");
        }

        return Ok(null);
    }

    [HttpPost("{jobId}")]
    public async Task<IActionResult> CloseJob(int jobId)
    {
        MetricsRegistry.JobApplicationsGetCounter.Inc();

        try
        {
            Console.WriteLine("POST /CloseJob endpoint hit");

            // Delete the job
            await _jobOfferService.DeleteJob(jobId);

            // If successful, delete the applications
            var response = await _apiGatewayClient.DeleteAsync($"applicationmanagement/api/applications/{jobId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete applications for job {jobId}");
            }

            return Ok("Job was closed successfully and applications for it were deleted");
        }
        catch (Exception e)
        {
            // Log the error for further investigation
            _logger.LogError(e, "Error occurred in CloseJob");

            // Return an error response
            return StatusCode(500, $"Something went wrong: {e.Message}");
        }
    }
}