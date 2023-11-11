using JobManagementService.Entities;
using JobManagementService.Services.JobOffer;
using Microsoft.AspNetCore.Mvc;

namespace JobManagementService.Controllers;

[Route("jobmanagement/api/joboffers")]
public class JobOfferController : Controller
{
    private readonly IJobOfferService _jobOfferService;
    private readonly ILogger<JobOfferController> _logger;

    public JobOfferController(IJobOfferService jobOfferService, ILogger<JobOfferController> logger)
    {
        _jobOfferService = jobOfferService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobOffer>> GetJobById(int id)
    {
        Console.WriteLine("GET /GetJobApplications endpoint hit");

        var job = await _jobOfferService.GetById(id);
        if (job == null)
            return NotFound();

        return Ok(job);
    }
}