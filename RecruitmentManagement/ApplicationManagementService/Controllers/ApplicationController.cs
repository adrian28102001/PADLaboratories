using ApplicationManagementService.Entities;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Repositories;

namespace ApplicationManagementService.Controllers;

public class ApplicationController : Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IRepository<Application> _repository;

        public ApplicationsController(IRepository<Application> repository)
        {
            _repository = repository;
        }

        // GET: api/Applications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Application>>> GetApplications()
        {
            return Ok(await _repository.GetAllAsync());
        }

        // GET: api/Applications/5
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

        // POST: api/Applications
        [HttpPost]
        public async Task<ActionResult<Application>> PostApplication(Application application)
        {
            await _repository.AddAsync(application);
            return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
        }

        // PUT: api/Applications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplication(int id, Application application)
        {
            if (id != application.Id)
            {
                return BadRequest();
            }

            try
            {
                await _repository.UpdateAsync(application);
            }
            catch // You might want to catch specific exceptions, such as not found exceptions
            {
                // If application with given ID does not exist
                if (await _repository.GetByIdAsync(id) == null)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/Applications/5
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
}