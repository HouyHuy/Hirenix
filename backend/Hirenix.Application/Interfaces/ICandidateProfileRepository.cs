using Hirenix.Domain.Entities;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Repository interface for candidate profile operations
/// </summary>
public interface ICandidateProfileRepository
{
    // Profile operations
    Task<CandidateProfile?> GetByUserIdAsync(ulong userId);
    Task<CandidateProfile?> GetByIdAsync(ulong id);
    Task<CandidateProfile> CreateAsync(CandidateProfile profile);
    Task<CandidateProfile> UpdateAsync(CandidateProfile profile);
    Task<bool> DeleteAsync(ulong id);
    Task<bool> ExistsByUserIdAsync(ulong userId);

    // Education operations
    Task<CandidateEducation> AddEducationAsync(CandidateEducation education);
    Task<CandidateEducation?> GetEducationByIdAsync(ulong id);
    Task<CandidateEducation> UpdateEducationAsync(CandidateEducation education);
    Task<bool> DeleteEducationAsync(ulong id);

    // Experience operations
    Task<CandidateExperience> AddExperienceAsync(CandidateExperience experience);
    Task<CandidateExperience?> GetExperienceByIdAsync(ulong id);
    Task<CandidateExperience> UpdateExperienceAsync(CandidateExperience experience);
    Task<bool> DeleteExperienceAsync(ulong id);

    // Skills operations
    Task AddSkillsAsync(ulong candidateId, List<CandidateSkill> skills);
    Task<bool> RemoveSkillAsync(ulong candidateId, uint skillId);
    Task<List<CandidateSkill>> GetSkillsByCandidateIdAsync(ulong candidateId);
}
