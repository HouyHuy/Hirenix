using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Job;

public class JobFilterDto
{
    public string? Search { get; set; }
    public uint? CityId { get; set; }
    public uint? IndustryId { get; set; }
    public WorkType? WorkType { get; set; }
    public JobLevel? Level { get; set; }
    public long? MinSalary { get; set; }
    public long? MaxSalary { get; set; }
    public JobStatus? Status { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
