using Hirenix.Application.DTOs.Company;
using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hirenix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Employer")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>
    /// Get all companies with pagination
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _companyService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get company by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(ulong id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return NotFound(new { message = $"Company with ID {id} not found" });

        return Ok(company);
    }

    /// <summary>
    /// Create a new company
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
    {
        try
        {
            var company = await _companyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing company
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateCompanyDto dto)
    {
        try
        {
            var company = await _companyService.UpdateAsync(id, dto);
            return Ok(company);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete company (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var result = await _companyService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = $"Company with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Get companies by industry
    /// </summary>
    [HttpGet("by-industry/{industryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIndustry(uint industryId)
    {
        var companies = await _companyService.GetByIndustryAsync(industryId);
        return Ok(companies);
    }

    /// <summary>
    /// Get companies by city
    /// </summary>
    [HttpGet("by-city/{cityId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCity(uint cityId)
    {
        var companies = await _companyService.GetByCityAsync(cityId);
        return Ok(companies);
    }
}
