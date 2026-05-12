using System.ComponentModel.DataAnnotations;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Candidate;

/// <summary>
/// Create candidate profile request
/// </summary>
public class CreateProfileDto
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    public Gender? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }

    public uint? CityId { get; set; }

    [Range(0, long.MaxValue, ErrorMessage = "Expected salary must be positive")]
    public long? ExpectedSalaryMin { get; set; }

    [Range(0, long.MaxValue, ErrorMessage = "Expected salary must be positive")]
    public long? ExpectedSalaryMax { get; set; }

    [MaxLength(100, ErrorMessage = "Desired position cannot exceed 100 characters")]
    public string? DesiredPosition { get; set; }

    public WorkType? WorkType { get; set; }

    public JobLevel? Level { get; set; }

    public uint? IndustryId { get; set; }

    public bool IsOpenToWork { get; set; } = true;

    [MaxLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
    public string? Bio { get; set; }

    /// <summary>
    /// List of skill IDs to add to profile
    /// </summary>
    public List<uint>? SkillIds { get; set; }
}
