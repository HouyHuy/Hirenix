using Hirenix.Application.DTOs.Candidate;
using Hirenix.Application.DTOs.Common;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Service interface for candidate profile business logic
/// </summary>
public interface ICandidateProfileService
{
    // Profile operations
    Task<ApiResponse<CandidateProfileDto>> GetMyProfileAsync(ulong userId);
    Task<ApiResponse<CandidateProfileDto>> CreateProfileAsync(ulong userId, CreateProfileDto dto);
    Task<ApiResponse<CandidateProfileDto>> UpdateProfileAsync(ulong userId, UpdateProfileDto dto);
    Task<ApiResponse<bool>> DeleteProfileAsync(ulong userId);

    // Education operations
    Task<ApiResponse<EducationDto>> AddEducationAsync(ulong userId, CreateEducationDto dto);
    Task<ApiResponse<EducationDto>> UpdateEducationAsync(ulong userId, ulong educationId, UpdateEducationDto dto);
    Task<ApiResponse<bool>> DeleteEducationAsync(ulong userId, ulong educationId);

    // Experience operations
    Task<ApiResponse<ExperienceDto>> AddExperienceAsync(ulong userId, CreateExperienceDto dto);
    Task<ApiResponse<ExperienceDto>> UpdateExperienceAsync(ulong userId, ulong experienceId, UpdateExperienceDto dto);
    Task<ApiResponse<bool>> DeleteExperienceAsync(ulong userId, ulong experienceId);

    // Skills operations
    Task<ApiResponse<List<CandidateSkillDto>>> AddSkillsAsync(ulong userId, AddSkillsDto dto);
    Task<ApiResponse<bool>> RemoveSkillAsync(ulong userId, uint skillId);
}
