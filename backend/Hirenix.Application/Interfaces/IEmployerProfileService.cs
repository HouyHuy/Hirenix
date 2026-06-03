using Hirenix.Application.DTOs.EmployerProfile;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Service interface for EmployerProfile business logic
/// </summary>
public interface IEmployerProfileService
{
    Task<EmployerProfileDto?> GetByIdAsync(ulong id);
    Task<EmployerProfileDto?> GetByUserIdAsync(ulong userId);
    Task<List<EmployerProfileDto>> GetByCompanyIdAsync(ulong companyId);
    Task<EmployerProfileDto> CreateAsync(ulong userId, CreateEmployerProfileDto dto);
    Task<EmployerProfileDto> UpdateAsync(ulong id, UpdateEmployerProfileDto dto);
    Task<bool> DeleteAsync(ulong id);
}
