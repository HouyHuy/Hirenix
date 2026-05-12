using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Candidate;

/// <summary>
/// Complete candidate profile data transfer object
/// </summary>
public class CandidateProfileDto
{
    public ulong Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public uint? CityId { get; set; }
    public string? CityName { get; set; }
    public long? ExpectedSalaryMin { get; set; }
    public long? ExpectedSalaryMax { get; set; }
    public string? DesiredPosition { get; set; }
    public WorkType? WorkType { get; set; }
    public JobLevel? Level { get; set; }
    public uint? IndustryId { get; set; }
    public string? IndustryName { get; set; }
    public bool IsOpenToWork { get; set; }
    public bool IsProfileHidden { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<EducationDto> Educations { get; set; } = new();
    public List<ExperienceDto> Experiences { get; set; } = new();
    public List<CandidateSkillDto> Skills { get; set; } = new();
}

/// <summary>
/// Skill with proficiency level
/// </summary>
public class CandidateSkillDto
{
    public ulong Id { get; set; }
    public uint SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public SkillLevel? Level { get; set; }
}
