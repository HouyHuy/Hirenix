using Hirenix.Domain.Enums;

namespace Hirenix.Domain.Entities;

public class CandidateProfile
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public uint? CityId { get; set; }
    public long? ExpectedSalaryMin { get; set; }
    public long? ExpectedSalaryMax { get; set; }
    public string? DesiredPosition { get; set; }
    public WorkType? WorkType { get; set; }
    public JobLevel? Level { get; set; }
    public uint? IndustryId { get; set; }
    public bool IsOpenToWork { get; set; } = true;
    public bool IsProfileHidden { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
    public Location? City { get; set; }
    public Industry? Industry { get; set; }
    public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
    public ICollection<CandidateEducation> Educations { get; set; } = new List<CandidateEducation>();
    public ICollection<CandidateExperience> Experiences { get; set; } = new List<CandidateExperience>();
}

public class CandidateSkill
{
    public ulong Id { get; set; }
    public ulong CandidateId { get; set; }
    public uint SkillId { get; set; }
    public SkillLevel? Level { get; set; }

    // Navigation
    public CandidateProfile? Candidate { get; set; }
    public Skill? Skill { get; set; }
}

public class CandidateEducation
{
    public ulong Id { get; set; }
    public ulong CandidateId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? Degree { get; set; }
    public string? Major { get; set; }
    public ushort StartYear { get; set; }
    public ushort? EndYear { get; set; }
    public string? Description { get; set; }

    // Navigation
    public CandidateProfile? Candidate { get; set; }
}

public class CandidateExperience
{
    public ulong Id { get; set; }
    public ulong CandidateId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }

    // Navigation
    public CandidateProfile? Candidate { get; set; }
}
