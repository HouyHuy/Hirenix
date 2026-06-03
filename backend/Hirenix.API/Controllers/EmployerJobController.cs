using Hirenix.Application.DTOs.Job;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/employer/jobs")]
[Authorize(Roles = "Employer")]
public class EmployerJobController : ControllerBase
{
    private readonly IEmployerJobService _employerJobService;
    private readonly ILogger<EmployerJobController> _logger;

    public EmployerJobController(
        IEmployerJobService employerJobService,
        ILogger<EmployerJobController> logger)
    {
        _employerJobService = employerJobService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new job posting
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployerJobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployerJobDto>> CreateJob([FromBody] CreateJobDto dto)
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var job = await _employerJobService.CreateJobAsync(userId, dto);
            
            _logger.LogInformation("Job {JobId} created by user {UserId}", job.Id, userId);
            
            return CreatedAtAction(
                nameof(GetJobById),
                new { id = job.Id },
                job);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized job creation attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid job creation data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            return StatusCode(500, new { message = "An error occurred while creating the job" });
        }
    }

    /// <summary>
    /// Get all jobs posted by the employer
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployerJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<EmployerJobDto>>> GetMyJobs([FromQuery] ulong? companyId = null, [FromQuery] JobStatus? status = null)
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var jobs = await _employerJobService.GetMyJobsAsync(userId, companyId, status);
            
            return Ok(jobs);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jobs for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return StatusCode(500, new { message = "An error occurred while retrieving jobs" });
        }
    }

    /// <summary>
    /// Get a specific job by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployerJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployerJobDto>> GetJobById(ulong id)
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var job = await _employerJobService.GetJobByIdAsync(userId, id);
            
            if (job == null)
            {
                return NotFound(new { message = "Job not found" });
            }
            
            return Ok(job);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to job {JobId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job {JobId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the job" });
        }
    }

    /// <summary>
    /// Update an existing job
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EmployerJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployerJobDto>> UpdateJob(ulong id, [FromBody] UpdateJobDto dto)
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var job = await _employerJobService.UpdateJobAsync(userId, id, dto);
            
            if (job == null)
            {
                return NotFound(new { message = "Job not found" });
            }
            
            _logger.LogInformation("Job {JobId} updated by user {UserId}", id, userId);
            
            return Ok(job);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized job update attempt for job {JobId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid job update data for job {JobId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the job" });
        }
    }

    /// <summary>
    /// Close a job posting
    /// </summary>
    [HttpPost("{id}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseJob(ulong id)
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _employerJobService.CloseJobAsync(userId, id);
            
            if (!success)
            {
                return NotFound(new { message = "Job not found" });
            }
            
            _logger.LogInformation("Job {JobId} closed by user {UserId}", id, userId);
            
            return Ok(new { message = "Job closed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized job close attempt for job {JobId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid job close operation for job {JobId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing job {JobId}", id);
            return StatusCode(500, new { message = "An error occurred while closing the job" });
        }
    }

    /// <summary>
    /// Delete a job posting
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteJob(ulong id)
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _employerJobService.DeleteJobAsync(userId, id);
            
            if (!success)
            {
                return NotFound(new { message = "Job not found" });
            }
            
            _logger.LogInformation("Job {JobId} deleted by user {UserId}", id, userId);
            
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized job delete attempt for job {JobId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid job delete operation for job {JobId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job {JobId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the job" });
        }
    }
}
