using System.ComponentModel.DataAnnotations;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Job;

/// <summary>
/// DTO for creating a new job posting
/// </summary>
public class CreateJobDto
{
    [Required(ErrorMessage = "Job title is required")]
    [StringLength(200, ErrorMessage = "Job title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Job description is required")]
    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requirements are required")]
    [StringLength(3000, ErrorMessage = "Requirements cannot exceed 3000 characters")]
    public string Requirements { get; set; } = string.Empty;

    [StringLength(3000, ErrorMessage = "Responsibilities cannot exceed 3000 characters")]
    public string? Responsibilities { get; set; }

    [Required(ErrorMessage = "Industry is required")]
    public uint IndustryId { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public uint LocationId { get; set; }

    [Required(ErrorMessage = "Work type is required")]
    public WorkType WorkType { get; set; }

    [Required(ErrorMessage = "Job level is required")]
    public JobLevel Level { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Minimum salary must be non-negative")]
    public decimal? SalaryMin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Maximum salary must be non-negative")]
    public decimal? SalaryMax { get; set; }

    public bool IsRemote { get; set; }

    [Required(ErrorMessage = "Expiry date is required")]
    public DateOnly ExpiryDate { get; set; }

    public List<uint> SkillIds { get; set; } = new();
}
