using System.ComponentModel.DataAnnotations;

namespace Hirenix.Application.DTOs.Candidate;

/// <summary>
/// Work experience DTO
/// </summary>
public class ExperienceDto
{
    public ulong Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Create experience request
/// </summary>
public class CreateExperienceDto
{
    [Required(ErrorMessage = "Company name is required")]
    [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position is required")]
    [MaxLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }
}

/// <summary>
/// Update experience request
/// </summary>
public class UpdateExperienceDto
{
    [Required(ErrorMessage = "Company name is required")]
    [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position is required")]
    [MaxLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }
}
