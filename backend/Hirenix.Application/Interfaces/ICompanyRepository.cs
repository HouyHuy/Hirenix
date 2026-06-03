using Hirenix.Domain.Entities;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Repository interface for Company entity operations
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Get company by ID with related data
    /// </summary>
    Task<Company?> GetByIdAsync(ulong id);

    /// <summary>
    /// Get company by name (for uniqueness check)
    /// </summary>
    Task<Company?> GetByNameAsync(string name);

    /// <summary>
    /// Get all companies with pagination
    /// </summary>
    Task<(List<Company> Companies, int Total)> GetAllAsync(int page, int pageSize);

    /// <summary>
    /// Create a new company
    /// </summary>
    Task<Company> CreateAsync(Company company);

    /// <summary>
    /// Update existing company
    /// </summary>
    Task<Company> UpdateAsync(Company company);

    /// <summary>
    /// Delete company (soft delete by setting IsActive = false)
    /// </summary>
    Task<bool> DeleteAsync(ulong id);

    /// <summary>
    /// Check if company name already exists (for validation)
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, ulong? excludeId = null);

    /// <summary>
    /// Get companies by industry
    /// </summary>
    Task<List<Company>> GetByIndustryAsync(uint industryId);

    /// <summary>
    /// Get companies by city
    /// </summary>
    Task<List<Company>> GetByCityAsync(uint cityId);
}
