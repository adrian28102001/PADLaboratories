using JobManagementService.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JobManagementService.Controllers;

[Route("api/jobs/applications")]
public class ApplicationController : ControllerBase
{
    private readonly HttpClient _apiGatewayClient;

    public ApplicationController(IHttpClientFactory clientFactory)
    {
        _apiGatewayClient = clientFactory.CreateClient("APIGateway");
    }

    [HttpGet("{jobId}")]
    public async Task<OkObjectResult> GetJobApplications(int jobId)
    {
        try
        {
            var response = await _apiGatewayClient.GetAsync($"/api/applications/{jobId}");

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