using Hirenix.Application.DTOs.EmployerProfile;
using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Employer")]
public class EmployerProfileController : ControllerBase
{
    private readonly IEmployerProfileService _profileService;

    public EmployerProfileController(IEmployerProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Get employer profile by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var profile = await _profileService.GetByIdAsync(id);
        if (profile == null)
            return NotFound(new { message = $"Employer profile with ID {id} not found" });

        return Ok(profile);
    }

    /// <summary>
    /// Get current user's employer profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user ID" });
        }

        var profile = await _profileService.GetByUserIdAsync(userId);
        if (profile == null)
            return NotFound(new { message = "Employer profile not found" });

        return Ok(profile);
    }

    /// <summary>
    /// Get all employer profiles by company ID
    /// </summary>
    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetByCompanyId(ulong companyId)
    {
        var profiles = await _profileService.GetByCompanyIdAsync(companyId);
        return Ok(profiles);
    }

    /// <summary>
    /// Create employer profile for current user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployerProfileDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !ulong.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user ID" });
        }

        try
        {
            var profile = await _profileService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update employer profile
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateEmployerProfileDto dto)
    {
        try
        {
            var profile = await _profileService.UpdateAsync(id, dto);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete employer profile
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var result = await _profileService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = $"Employer profile with ID {id} not found" });

        return NoContent();
    }
}
