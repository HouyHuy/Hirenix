using Hirenix.Application.DTOs.Company;
using Hirenix.Application.DTOs.Job;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Service interface for Company business logic
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Get company by ID
    /// </summary>
    Task<CompanyDto?> GetByIdAsync(ulong id);

    /// <summary>
    /// Get all companies with pagination
    /// </summary>
    Task<PaginatedResultDto<CompanyDto>> GetAllAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Create a new company
    /// </summary>
    Task<CompanyDto> CreateAsync(CreateCompanyDto dto);

    /// <summary>
    /// Update existing company
    /// </summary>
    Task<CompanyDto> UpdateAsync(ulong id, UpdateCompanyDto dto);

    /// <summary>
    /// Delete company (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(ulong id);

    /// <summary>
    /// Get companies by industry
    /// </summary>
    Task<List<CompanyDto>> GetByIndustryAsync(uint industryId);

    /// <summary>
    /// Get companies by city
    /// </summary>
    Task<List<CompanyDto>> GetByCityAsync(uint cityId);
}
