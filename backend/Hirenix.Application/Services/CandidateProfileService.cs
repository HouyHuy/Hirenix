using Hirenix.Application.DTOs.Candidate;
using Hirenix.Application.DTOs.Common;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;

namespace Hirenix.Application.Services;

/// <summary>
/// Service implementation for candidate profile business logic
/// </summary>
public class CandidateProfileService : ICandidateProfileService
{
    private readonly ICandidateProfileRepository _repository;

    public CandidateProfileService(ICandidateProfileRepository repository)
    {
        _repository = repository;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  PROFILE OPERATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<CandidateProfileDto>> GetMyProfileAsync(ulong userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<CandidateProfileDto>.Fail("Profile not found");

        var dto = MapToProfileDto(profile);
        return ApiResponse<CandidateProfileDto>.Ok(dto);
    }

    public async Task<ApiResponse<CandidateProfileDto>> CreateProfileAsync(ulong userId, CreateProfileDto dto)
    {
        // Check if profile already exists
        var exists = await _repository.ExistsByUserIdAsync(userId);
        if (exists)
            return ApiResponse<CandidateProfileDto>.Fail("Profile already exists for this user");

        // Create profile entity
        var profile = new CandidateProfile
        {
            UserId = userId,
            FullName = dto.FullName,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Address = dto.Address,
            CityId = dto.CityId,
            ExpectedSalaryMin = dto.ExpectedSalaryMin,
            ExpectedSalaryMax = dto.ExpectedSalaryMax,
            DesiredPosition = dto.DesiredPosition,
            WorkType = dto.WorkType,
            Level = dto.Level,
            IndustryId = dto.IndustryId,
            IsOpenToWork = dto.IsOpenToWork,
            Bio = dto.Bio
        };

        // Create profile
        var created = await _repository.CreateAsync(profile);

        // Add skills if provided
        if (dto.SkillIds != null && dto.SkillIds.Any())
        {
            var skills = dto.SkillIds.Select(skillId => new CandidateSkill
            {
                CandidateId = created.Id,
                SkillId = skillId
            }).ToList();

            await _repository.AddSkillsAsync(created.Id, skills);
        }

        // Reload profile with all related data
        var result = await _repository.GetByUserIdAsync(userId);
        var resultDto = MapToProfileDto(result!);
        
        return ApiResponse<CandidateProfileDto>.Ok(resultDto, "Profile created successfully");
    }

    public async Task<ApiResponse<CandidateProfileDto>> UpdateProfileAsync(ulong userId, UpdateProfileDto dto)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<CandidateProfileDto>.Fail("Profile not found");

        // Update profile fields
        profile.FullName = dto.FullName;
        profile.Gender = dto.Gender;
        profile.DateOfBirth = dto.DateOfBirth;
        profile.Address = dto.Address;
        profile.CityId = dto.CityId;
        profile.ExpectedSalaryMin = dto.ExpectedSalaryMin;
        profile.ExpectedSalaryMax = dto.ExpectedSalaryMax;
        profile.DesiredPosition = dto.DesiredPosition;
        profile.WorkType = dto.WorkType;
        profile.Level = dto.Level;
        profile.IndustryId = dto.IndustryId;
        profile.IsOpenToWork = dto.IsOpenToWork;
        profile.IsProfileHidden = dto.IsProfileHidden;
        profile.Bio = dto.Bio;

        await _repository.UpdateAsync(profile);

        // Reload profile with all related data
        var updated = await _repository.GetByUserIdAsync(userId);
        var resultDto = MapToProfileDto(updated!);
        
        return ApiResponse<CandidateProfileDto>.Ok(resultDto, "Profile updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteProfileAsync(ulong userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<bool>.Fail("Profile not found");

        var deleted = await _repository.DeleteAsync(profile.Id);
        
        if (!deleted)
            return ApiResponse<bool>.Fail("Failed to delete profile");

        return ApiResponse<bool>.Ok(true, "Profile deleted successfully");
    }

    // ═══════════════════════════════════════════════════════════════════
    //  EDUCATION OPERATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<EducationDto>> AddEducationAsync(ulong userId, CreateEducationDto dto)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<EducationDto>.Fail("Profile not found");

        // Validate end year
        if (dto.EndYear.HasValue && dto.EndYear < dto.StartYear)
            return ApiResponse<EducationDto>.Fail("End year cannot be before start year");

        var education = new CandidateEducation
        {
            CandidateId = profile.Id,
            SchoolName = dto.SchoolName,
            Degree = dto.Degree,
            Major = dto.Major,
            StartYear = dto.StartYear,
            EndYear = dto.EndYear,
            Description = dto.Description
        };

        var created = await _repository.AddEducationAsync(education);
        var resultDto = MapToEducationDto(created);
        
        return ApiResponse<EducationDto>.Ok(resultDto, "Education added successfully");
    }

    public async Task<ApiResponse<EducationDto>> UpdateEducationAsync(ulong userId, ulong educationId, UpdateEducationDto dto)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<EducationDto>.Fail("Profile not found");

        var education = await _repository.GetEducationByIdAsync(educationId);
        
        if (education == null)
            return ApiResponse<EducationDto>.Fail("Education not found");

        // Verify ownership
        if (education.CandidateId != profile.Id)
            return ApiResponse<EducationDto>.Fail("You don't have permission to update this education");

        // Validate end year
        if (dto.EndYear.HasValue && dto.EndYear < dto.StartYear)
            return ApiResponse<EducationDto>.Fail("End year cannot be before start year");

        // Update fields
        education.SchoolName = dto.SchoolName;
        education.Degree = dto.Degree;
        education.Major = dto.Major;
        education.StartYear = dto.StartYear;
        education.EndYear = dto.EndYear;
        education.Description = dto.Description;

        var updated = await _repository.UpdateEducationAsync(education);
        var resultDto = MapToEducationDto(updated);
        
        return ApiResponse<EducationDto>.Ok(resultDto, "Education updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteEducationAsync(ulong userId, ulong educationId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<bool>.Fail("Profile not found");

        var education = await _repository.GetEducationByIdAsync(educationId);
        
        if (education == null)
            return ApiResponse<bool>.Fail("Education not found");

        // Verify ownership
        if (education.CandidateId != profile.Id)
            return ApiResponse<bool>.Fail("You don't have permission to delete this education");

        var deleted = await _repository.DeleteEducationAsync(educationId);
        
        if (!deleted)
            return ApiResponse<bool>.Fail("Failed to delete education");

        return ApiResponse<bool>.Ok(true, "Education deleted successfully");
    }

    // ═══════════════════════════════════════════════════════════════════
    //  EXPERIENCE OPERATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<ExperienceDto>> AddExperienceAsync(ulong userId, CreateExperienceDto dto)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<ExperienceDto>.Fail("Profile not found");

        // Validate dates
        if (!dto.IsCurrent && dto.EndDate.HasValue && dto.EndDate < dto.StartDate)
            return ApiResponse<ExperienceDto>.Fail("End date cannot be before start date");

        var experience = new CandidateExperience
        {
            CandidateId = profile.Id,
            CompanyName = dto.CompanyName,
            Position = dto.Position,
            StartDate = dto.StartDate,
            EndDate = dto.IsCurrent ? null : dto.EndDate,
            IsCurrent = dto.IsCurrent,
            Description = dto.Description
        };

        var created = await _repository.AddExperienceAsync(experience);
        var resultDto = MapToExperienceDto(created);
        
        return ApiResponse<ExperienceDto>.Ok(resultDto, "Experience added successfully");
    }

    public async Task<ApiResponse<ExperienceDto>> UpdateExperienceAsync(ulong userId, ulong experienceId, UpdateExperienceDto dto)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<ExperienceDto>.Fail("Profile not found");

        var experience = await _repository.GetExperienceByIdAsync(experienceId);
        
        if (experience == null)
            return ApiResponse<ExperienceDto>.Fail("Experience not found");

        // Verify ownership
        if (experience.CandidateId != profile.Id)
            return ApiResponse<ExperienceDto>.Fail("You don't have permission to update this experience");

        // Validate dates
        if (!dto.IsCurrent && dto.EndDate.HasValue && dto.EndDate < dto.StartDate)
            return ApiResponse<ExperienceDto>.Fail("End date cannot be before start date");

        // Update fields
        experience.CompanyName = dto.CompanyName;
        experience.Position = dto.Position;
        experience.StartDate = dto.StartDate;
        experience.EndDate = dto.IsCurrent ? null : dto.EndDate;
        experience.IsCurrent = dto.IsCurrent;
        experience.Description = dto.Description;

        var updated = await _repository.UpdateExperienceAsync(experience);
        var resultDto = MapToExperienceDto(updated);
        
        return ApiResponse<ExperienceDto>.Ok(resultDto, "Experience updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteExperienceAsync(ulong userId, ulong experienceId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<bool>.Fail("Profile not found");

        var experience = await _repository.GetExperienceByIdAsync(experienceId);
        
        if (experience == null)
            return ApiResponse<bool>.Fail("Experience not found");

        // Verify ownership
        if (experience.CandidateId != profile.Id)
            return ApiResponse<bool>.Fail("You don't have permission to delete this experience");

        var deleted = await _repository.DeleteExperienceAsync(experienceId);
        
        if (!deleted)
            return ApiResponse<bool>.Fail("Failed to delete experience");

        return ApiResponse<bool>.Ok(true, "Experience deleted successfully");
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SKILLS OPERATIONS
    // ═══════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<List<CandidateSkillDto>>> AddSkillsAsync(ulong userId, AddSkillsDto dto)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<List<CandidateSkillDto>>.Fail("Profile not found");

        // Create candidate skills
        var skills = dto.Skills.Select(s => new CandidateSkill
        {
            CandidateId = profile.Id,
            SkillId = s.SkillId,
            Level = s.Level
        }).ToList();

        await _repository.AddSkillsAsync(profile.Id, skills);

        // Reload skills
        var updatedSkills = await _repository.GetSkillsByCandidateIdAsync(profile.Id);
        var resultDto = updatedSkills.Select(MapToCandidateSkillDto).ToList();
        
        return ApiResponse<List<CandidateSkillDto>>.Ok(resultDto, "Skills added successfully");
    }

    public async Task<ApiResponse<bool>> RemoveSkillAsync(ulong userId, uint skillId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        
        if (profile == null)
            return ApiResponse<bool>.Fail("Profile not found");

        var removed = await _repository.RemoveSkillAsync(profile.Id, skillId);
        
        if (!removed)
            return ApiResponse<bool>.Fail("Skill not found or already removed");

        return ApiResponse<bool>.Ok(true, "Skill removed successfully");
    }

    // ═══════════════════════════════════════════════════════════════════
    //  MAPPING HELPERS
    // ═══════════════════════════════════════════════════════════════════

    private static CandidateProfileDto MapToProfileDto(CandidateProfile profile)
    {
        return new CandidateProfileDto
        {
            Id = profile.Id,
            FullName = profile.FullName,
            AvatarUrl = profile.AvatarUrl,
            Gender = profile.Gender,
            DateOfBirth = profile.DateOfBirth,
            Address = profile.Address,
            CityId = profile.CityId,
            CityName = profile.City?.Name,
            ExpectedSalaryMin = profile.ExpectedSalaryMin,
            ExpectedSalaryMax = profile.ExpectedSalaryMax,
            DesiredPosition = profile.DesiredPosition,
            WorkType = profile.WorkType,
            Level = profile.Level,
            IndustryId = profile.IndustryId,
            IndustryName = profile.Industry?.Name,
            IsOpenToWork = profile.IsOpenToWork,
            IsProfileHidden = profile.IsProfileHidden,
            Bio = profile.Bio,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
            Educations = profile.Educations.Select(MapToEducationDto).ToList(),
            Experiences = profile.Experiences.Select(MapToExperienceDto).ToList(),
            Skills = profile.Skills.Select(MapToCandidateSkillDto).ToList()
        };
    }

    private static EducationDto MapToEducationDto(CandidateEducation education)
    {
        return new EducationDto
        {
            Id = education.Id,
            SchoolName = education.SchoolName,
            Degree = education.Degree,
            Major = education.Major,
            StartYear = education.StartYear,
            EndYear = education.EndYear,
            Description = education.Description
        };
    }

    private static ExperienceDto MapToExperienceDto(CandidateExperience experience)
    {
        return new ExperienceDto
        {
            Id = experience.Id,
            CompanyName = experience.CompanyName,
            Position = experience.Position,
            StartDate = experience.StartDate,
            EndDate = experience.EndDate,
            IsCurrent = experience.IsCurrent,
            Description = experience.Description
        };
    }

    private static CandidateSkillDto MapToCandidateSkillDto(CandidateSkill skill)
    {
        return new CandidateSkillDto
        {
            Id = skill.Id,
            SkillId = skill.SkillId,
            SkillName = skill.Skill?.Name ?? string.Empty,
            Category = skill.Skill?.Category,
            Level = skill.Level
        };
    }
}
