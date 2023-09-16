using ApplicationManagementService.Entities;
using ApplicationManagementService.Extensions;
using ApplicationManagementService.Models;
using ApplicationManagementService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationManagementService.Controllers;

[Route("applicationmanagement/api/applications")]
[ApiController]
public class ApplicationController : ControllerBase
{
    private readonly IRepository<Application> _repository;

    public ApplicationController(IRepository<Application> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Application>>> GetApplications()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Application>> GetApplication(int id)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    [HttpGet("job/{jobId}")]
    public async Task<ActionResult<Application>> GetApplicationByJobId(int jobId)
    {
        var table = _repository.GetAllQuery();
        var application = await table.FirstOrDefaultAsync(it => it.JobOfferId == jobId);

        if (application == null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    [HttpPost]
    public async Task<ActionResult<Application>> PostApplication(ApplicationModel application)
    {
        await _repository.AddAsync(application.ToModel());
        return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutApplication(int id, Application application)
    {
        if (id != application.Id)
        {
            return BadRequest();
        }

        if (await _repository.GetByIdAsync(id) == null)
        {
            return NotFound();
        }

        try
        {
            await _repository.UpdateAsync(application);
        }
        catch (Exception ex)
        {
            // ignored
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(application);
        return NoContent();
    }
}