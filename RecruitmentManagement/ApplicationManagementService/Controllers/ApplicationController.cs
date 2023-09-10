using ApplicationManagementService.Entities;
using ApplicationManagementService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationManagementService.Controllers;

[Route("api/applications")]
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

    [HttpPost]
    public async Task<ActionResult<Application>> PostApplication(Application application)
    {
        await _repository.AddAsync(application);
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