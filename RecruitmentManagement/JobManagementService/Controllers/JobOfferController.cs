using JobManagementService.Entities;
using JobManagementService.Services.JobOffer;
using Microsoft.AspNetCore.Mvc;

namespace JobManagementService.Controllers;

[Route("jobmanagement/api/joboffers")]
public class JobOfferController : Controller
{
    private readonly IJobOfferService _jobOfferService;

    public JobOfferController(IJobOfferService jobOfferService)
    {
        _jobOfferService = jobOfferService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobOffer>> GetJobById(int id)
    {
        var job = await _jobOfferService.GetById(id);
        if (job == null)
            return NotFound();

        return Ok(job);
    }
}