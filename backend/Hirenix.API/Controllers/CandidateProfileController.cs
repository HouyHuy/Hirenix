using System.Security.Claims;
using Hirenix.Application.DTOs.Candidate;
using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hirenix.API.Controllers;

/// <summary>
/// Controller for candidate profile management
/// </summary>
[ApiController]
[Route("api/candidate")]
[Authorize(Roles = "Candidate")]
public class CandidateProfileController : ControllerBase
{
    private readonly ICandidateProfileService _service;

    public CandidateProfileController(ICandidateProfileService service)
    {
        _service = service;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  PROFILE ENDPOINTS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get my candidate profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        var result = await _service.GetMyProfileAsync(userId);
        
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create candidate profile
    /// </summary>
    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
    {
        var userId = GetUserId();
        var result = await _service.CreateProfileAsync(userId, dto);
        
        return result.Success 
            ? CreatedAtAction(nameof(GetMyProfile), result) 
            : BadRequest(result);
    }

    /// <summary>
    /// Update candidate profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var result = await _service.UpdateProfileAsync(userId, dto);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete candidate profile
    /// </summary>
    [HttpDelete("profile")]
    public async Task<IActionResult> DeleteProfile()
    {
        var userId = GetUserId();
        var result = await _service.DeleteProfileAsync(userId);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  EDUCATION ENDPOINTS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Add education to profile
    /// </summary>
    [HttpPost("education")]
    public async Task<IActionResult> AddEducation([FromBody] CreateEducationDto dto)
    {
        var userId = GetUserId();
        var result = await _service.AddEducationAsync(userId, dto);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update education
    /// </summary>
    [HttpPut("education/{id}")]
    public async Task<IActionResult> UpdateEducation(ulong id, [FromBody] UpdateEducationDto dto)
    {
        var userId = GetUserId();
        var result = await _service.UpdateEducationAsync(userId, id, dto);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete education
    /// </summary>
    [HttpDelete("education/{id}")]
    public async Task<IActionResult> DeleteEducation(ulong id)
    {
        var userId = GetUserId();
        var result = await _service.DeleteEducationAsync(userId, id);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  EXPERIENCE ENDPOINTS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Add experience to profile
    /// </summary>
    [HttpPost("experience")]
    public async Task<IActionResult> AddExperience([FromBody] CreateExperienceDto dto)
    {
        var userId = GetUserId();
        var result = await _service.AddExperienceAsync(userId, dto);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update experience
    /// </summary>
    [HttpPut("experience/{id}")]
    public async Task<IActionResult> UpdateExperience(ulong id, [FromBody] UpdateExperienceDto dto)
    {
        var userId = GetUserId();
        var result = await _service.UpdateExperienceAsync(userId, id, dto);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete experience
    /// </summary>
    [HttpDelete("experience/{id}")]
    public async Task<IActionResult> DeleteExperience(ulong id)
    {
        var userId = GetUserId();
        var result = await _service.DeleteExperienceAsync(userId, id);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SKILLS ENDPOINTS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Add skills to profile
    /// </summary>
    [HttpPost("skills")]
    public async Task<IActionResult> AddSkills([FromBody] AddSkillsDto dto)
    {
        var userId = GetUserId();
        var result = await _service.AddSkillsAsync(userId, dto);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Remove skill from profile
    /// </summary>
    [HttpDelete("skills/{skillId}")]
    public async Task<IActionResult> RemoveSkill(uint skillId)
    {
        var userId = GetUserId();
        var result = await _service.RemoveSkillAsync(userId, skillId);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════

    private ulong GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return ulong.Parse(userIdClaim ?? "0");
    }
}
