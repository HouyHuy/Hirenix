using Hirenix.Application.DTOs.Application;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/employer/applications")]
[Authorize(Roles = "Employer")]
public class EmployerApplicationsController : ControllerBase
{
    private readonly IEmployerApplicationService _employerApplicationService;
    private readonly ILogger<EmployerApplicationsController> _logger;

    public EmployerApplicationsController(
        IEmployerApplicationService employerApplicationService,
        ILogger<EmployerApplicationsController> logger)
    {
        _employerApplicationService = employerApplicationService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EmployerApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<EmployerApplicationDto>>> GetApplications(
        [FromQuery] ulong? jobId = null,
        [FromQuery] ApplicationStatus? status = null)
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var applications = await _employerApplicationService.GetApplicationsAsync(userId, jobId, status);
            return Ok(applications);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized ATS list access: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employer applications");
            return StatusCode(500, new { message = "An error occurred while retrieving applications" });
        }
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(EmployerApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployerApplicationDto>> GetApplicationById(long id)
    {
        try
        {
            if (id < 0)
            {
                return BadRequest(new { message = "Invalid application id" });
            }

            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var application = await _employerApplicationService.GetApplicationByIdAsync(userId, (ulong)id);
            if (application == null)
            {
                return NotFound(new { message = "Application not found" });
            }

            return Ok(application);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized ATS detail access for application {ApplicationId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application {ApplicationId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving application detail" });
        }
    }

    [HttpPut("{id:long}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateApplicationStatusDto dto)
    {
        try
        {
            if (id < 0)
            {
                return BadRequest(new { message = "Invalid application id" });
            }

            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _employerApplicationService.UpdateApplicationStatusAsync(userId, (ulong)id, dto.Status, dto.ReviewNotes);
            if (!success)
            {
                return NotFound(new { message = "Application not found" });
            }

            return Ok(new { message = "Application status updated successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized ATS status update for application {ApplicationId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid status update for application {ApplicationId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for application {ApplicationId}", id);
            return StatusCode(500, new { message = "An error occurred while updating application status" });
        }
    }

    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApplicationStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApplicationStatisticsDto>> GetStatistics()
    {
        try
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var stats = await _employerApplicationService.GetStatisticsAsync(userId);
            return Ok(stats);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized ATS statistics access: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ATS statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving statistics" });
        }
    }
}
