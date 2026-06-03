using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hirenix.API.Controllers;

/// <summary>
/// Controller for taxonomy data (skills, industries, locations)
/// </summary>
[ApiController]
[Route("api/taxonomy")]
public class TaxonomyController : ControllerBase
{
    private readonly ITaxonomyService _taxonomyService;

    public TaxonomyController(ITaxonomyService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    /// <summary>
    /// Get all skills
    /// </summary>
    [HttpGet("skills")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllSkills()
    {
        var result = await _taxonomyService.GetAllSkillsAsync();
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all industries
    /// </summary>
    [HttpGet("industries")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllIndustries()
    {
        var result = await _taxonomyService.GetAllIndustriesAsync();
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all locations
    /// </summary>
    [HttpGet("locations")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllLocations()
    {
        var result = await _taxonomyService.GetAllLocationsAsync();
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
