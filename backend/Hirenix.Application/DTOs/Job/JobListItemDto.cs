using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Job;

public class JobListItemDto
{
    public ulong Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogo { get; set; }
    public string? CityName { get; set; }
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
    public List<string> Skills { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
