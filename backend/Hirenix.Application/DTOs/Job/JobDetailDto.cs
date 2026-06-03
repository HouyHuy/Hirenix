using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Job;

public class JobDetailDto
{
    public ulong Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public string? Benefits { get; set; }
    public WorkType WorkType { get; set; }
    public JobLevel Level { get; set; }
    public long? SalaryMin { get; set; }
    public long? SalaryMax { get; set; }
    public bool IsSalaryVisible { get; set; }
    public DateOnly Deadline { get; set; }
    public JobStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public uint ViewsCount { get; set; }
    public uint ApplicationsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Company info
    public CompanyInfoDto Company { get; set; } = new();
    
    // Location
    public string? City { get; set; }
    
    // Industry
    public string? Industry { get; set; }
    
    // Skills
    public List<JobSkillDto> Skills { get; set; } = new();
    
    // Application status for current user
    public bool HasApplied { get; set; }
}

public class CompanyInfoDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
}

public class JobSkillDto
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}
