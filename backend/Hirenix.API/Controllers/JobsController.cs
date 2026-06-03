using Hirenix.Application.DTOs.Job;
using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>
    /// Get paginated list of jobs with optional filters
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetJobs([FromQuery] JobFilterDto filter)
    {
        var result = await _jobService.GetJobsAsync(filter);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get job details by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetJobById(ulong id)
    {
        var result = await _jobService.GetJobByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get detailed job information with company details and skills
    /// </summary>
    [HttpGet("{id}/detail")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetJobDetail(ulong id)
    {
        // Get userId from claims
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("uid")?.Value;
        ulong? userId = null;
        if (ulong.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _jobService.GetJobDetailAsync(id, userId);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get available filter options (cities, industries, work types, levels)
    /// </summary>
    [HttpGet("filters")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFilterOptions()
    {
        var result = await _jobService.GetFilterOptionsAsync();
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
