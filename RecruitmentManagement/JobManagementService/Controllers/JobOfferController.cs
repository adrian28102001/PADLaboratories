using JobManagementService.Entities;
using JobManagementService.Services.JobOffer;
using Microsoft.AspNetCore.Mvc;

namespace JobManagementService.Controllers;

[Route("api/joboffers")]
public class JobOfferController : Controller
{
    private readonly JobOfferService _jobOfferService;

    public JobOfferController(JobOfferService jobOfferService)
    {
        _jobOfferService = jobOfferService;
    }

    [HttpGet("{id}")]
    public ActionResult<JobOffer> GetJobById(int id)
    {
        var job = _jobOfferService.GetById(id);
        if(job == null)
            return NotFound();
            
        return Ok(job);
    }
}