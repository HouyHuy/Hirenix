using Hirenix.Domain.Enums;

namespace Hirenix.Domain.Entities;

public class Job
{
    public ulong Id { get; set; }
    public ulong CompanyId { get; set; }
    public ulong CreatedBy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public string? Benefits { get; set; }
    public WorkType WorkType { get; set; }
    public JobLevel Level { get; set; }
    public long? SalaryMin { get; set; }
    public long? SalaryMax { get; set; }
    public bool IsSalaryVisible { get; set; } = true;
    public uint? CityId { get; set; }
    public uint? IndustryId { get; set; }
    public DateOnly Deadline { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Draft;
    public bool IsFeatured { get; set; }
    public uint ViewsCount { get; set; }
    public uint ApplicationsCount { get; set; }
    public ulong? ParentJobId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Company? Company { get; set; }
    public EmployerProfile? Creator { get; set; }
    public Location? City { get; set; }
    public Industry? Industry { get; set; }
    public Job? ParentJob { get; set; }
    public ICollection<JobSkill> Skills { get; set; } = new List<JobSkill>();
}

public class JobSkill
{
    public ulong Id { get; set; }
    public ulong JobId { get; set; }
    public uint SkillId { get; set; }
    public bool IsRequired { get; set; } = true;

    // Navigation
    public Job? Job { get; set; }
    public Skill? Skill { get; set; }
}
