using ApplicationManagementService.Entities;
using ApplicationManagementService.Extensions;
using ApplicationManagementService.Metric;
using ApplicationManagementService.Models;
using ApplicationManagementService.Repositories;
using ApplicationManagementService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ApplicationManagementService.Controllers;

[Route("applicationmanagement/api/applications")]
[ApiController]
public class ApplicationController : ControllerBase
{
    private readonly IRepository<Application> _repository;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<ApplicationController> _logger;

    public ApplicationController(IRepository<Application> repository, IEmailService emailService,
        IFileStorageService fileStorageService, IOptions<EmailSettings> emailSettings,
        ILogger<ApplicationController> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Application>>> GetApplications()
    {
        MetricsRegistry.ApplicationsGetCounter.Inc();
        _logger.LogInformation("GET /applications endpoint hit");

        return Ok(await _repository.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Application>> GetApplication(int id)
    {
        MetricsRegistry.ApplicationGetByIdCounter.Inc();
        _logger.LogInformation("GET /applications/id endpoint hit");

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
        MetricsRegistry.ApplicationGetByJobIdCounter.Inc();
        _logger.LogInformation("GET /job/id endpoint hit");

        var table = _repository.GetAllQuery();
        var application = await table.FirstOrDefaultAsync(it => it.JobOfferId == jobId);

        if (application == null)
        {
            return NotFound();
        }

        return Ok(application);
    }

    [HttpPost]
    public async Task<ActionResult<Application>> PostApplication([FromForm] ApplicationModel application)
    {
        MetricsRegistry.ApplicationPostCounter.Inc();
        _logger.LogInformation("POST /postapplication endpoint hit");

        if (application.CVFile != null && application.CVFile.Length > 0)
        {
            var filePath = await _fileStorageService.SaveFileAsync(application.CVFile);
            application.CVPath = filePath;
        }

        await _repository.AddAsync(application.ToModel());

        // SendEmail(application);

        return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> PutApplication(int id, Application application)
    {
        MetricsRegistry.ApplicationPutCounter.Inc();
        _logger.LogInformation("PUT /putapplication endpoint hit");

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
        MetricsRegistry.ApplicationDeleteCounter.Inc();
        _logger.LogInformation("DELETE /deleteapplication endpoint hit");

        var application = await _repository.GetByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(application);
        return NoContent();
    }

    private void SendEmail(ApplicationModel application)
    {
        if (application.CVFile != null && application.CVFile.Length > 0)
        {
            Task.Run(() => _emailService.SendEmailWithAttachmentAsync(
                _emailSettings.Recipient,
                $"New application received from CandidateId: {application.CandidateId} for JobOfferId: {application.JobOfferId}",
                application.CVPath
            )).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine($"Failed to send email. Exception: {task.Exception?.InnerException?.Message}");
                }
            });
        }
    }
}