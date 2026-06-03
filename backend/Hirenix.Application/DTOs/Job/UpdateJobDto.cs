using System.ComponentModel.DataAnnotations;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Job;

/// <summary>
/// DTO for updating an existing job posting
/// All fields are optional for partial updates
/// </summary>
public class UpdateJobDto
{
    [StringLength(200, ErrorMessage = "Job title cannot exceed 200 characters")]
    public string? Title { get; set; }

    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string? Description { get; set; }

    [StringLength(3000, ErrorMessage = "Requirements cannot exceed 3000 characters")]
    public string? Requirements { get; set; }

    [StringLength(3000, ErrorMessage = "Responsibilities cannot exceed 3000 characters")]
    public string? Responsibilities { get; set; }

    public uint? IndustryId { get; set; }

    public uint? LocationId { get; set; }

    public WorkType? WorkType { get; set; }

    public JobLevel? Level { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Minimum salary must be non-negative")]
    public decimal? SalaryMin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Maximum salary must be non-negative")]
    public decimal? SalaryMax { get; set; }

    public bool? IsRemote { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public List<uint>? SkillIds { get; set; }
}
