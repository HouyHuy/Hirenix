using Hirenix.Application.DTOs.EmployerProfile;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;

namespace Hirenix.Application.Services;

/// <summary>
/// Service implementation for EmployerProfile business logic
/// </summary>
public class EmployerProfileService : IEmployerProfileService
{
    private readonly IEmployerProfileRepository _profileRepository;
    private readonly ICompanyRepository _companyRepository;

    public EmployerProfileService(
        IEmployerProfileRepository profileRepository,
        ICompanyRepository companyRepository)
    {
        _profileRepository = profileRepository;
        _companyRepository = companyRepository;
    }

    public async Task<EmployerProfileDto?> GetByIdAsync(ulong id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        return profile == null ? null : MapToDto(profile);
    }

    public async Task<EmployerProfileDto?> GetByUserIdAsync(ulong userId)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);
        return profile == null ? null : MapToDto(profile);
    }

    public async Task<List<EmployerProfileDto>> GetByCompanyIdAsync(ulong companyId)
    {
        var profiles = await _profileRepository.GetByCompanyIdAsync(companyId);
        return profiles.Select(MapToDto).ToList();
    }

    public async Task<EmployerProfileDto> CreateAsync(ulong userId, CreateEmployerProfileDto dto)
    {
        // Check if user already has an employer profile
        if (await _profileRepository.ExistsByUserIdAsync(userId))
        {
            throw new InvalidOperationException("User already has an employer profile");
        }

        // Validate company exists
        var company = await _companyRepository.GetByIdAsync(dto.CompanyId);
        if (company == null)
        {
            throw new KeyNotFoundException($"Company with ID {dto.CompanyId} not found");
        }

        var profile = new EmployerProfile
        {
            UserId = userId,
            CompanyId = dto.CompanyId,
            FullName = dto.FullName,
            Title = dto.Title,
            IsAdmin = dto.IsAdmin
        };

        var created = await _profileRepository.CreateAsync(profile);
        
        // Reload with navigation properties
        var result = await _profileRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<EmployerProfileDto> UpdateAsync(ulong id, UpdateEmployerProfileDto dto)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        if (profile == null)
        {
            throw new KeyNotFoundException($"Employer profile with ID {id} not found");
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(dto.FullName))
            profile.FullName = dto.FullName;
        
        if (dto.Title != null)
            profile.Title = dto.Title;
        
        if (dto.IsAdmin.HasValue)
            profile.IsAdmin = dto.IsAdmin.Value;

        var updated = await _profileRepository.UpdateAsync(profile);
        
        // Reload with navigation properties
        var result = await _profileRepository.GetByIdAsync(updated.Id);
        return MapToDto(result!);
    }

    public async Task<bool> DeleteAsync(ulong id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        if (profile == null)
        {
            return false;
        }

        return await _profileRepository.DeleteAsync(id);
    }

    private EmployerProfileDto MapToDto(EmployerProfile profile)
    {
        return new EmployerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            UserEmail = profile.User?.Email ?? string.Empty,
            CompanyId = profile.CompanyId,
            CompanyName = profile.Company?.Name ?? string.Empty,
            FullName = profile.FullName,
            Title = profile.Title,
            IsAdmin = profile.IsAdmin,
            IsActive = profile.IsActive,
            CreatedAt = profile.CreatedAt
        };
    }
}
