using Hirenix.Domain.Entities;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Repository interface for EmployerProfile entity operations
/// </summary>
public interface IEmployerProfileRepository
{
    Task<EmployerProfile?> GetByIdAsync(ulong id);
    Task<EmployerProfile?> GetByUserIdAsync(ulong userId);
    Task<List<EmployerProfile>> GetByCompanyIdAsync(ulong companyId);
    Task<EmployerProfile> CreateAsync(EmployerProfile profile);
    Task<EmployerProfile> UpdateAsync(EmployerProfile profile);
    Task<bool> DeleteAsync(ulong id);
    Task<bool> ExistsByUserIdAsync(ulong userId);
}
