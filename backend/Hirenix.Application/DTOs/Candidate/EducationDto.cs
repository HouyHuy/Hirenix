using System.ComponentModel.DataAnnotations;

namespace Hirenix.Application.DTOs.Candidate;

/// <summary>
/// Education record DTO
/// </summary>
public class EducationDto
{
    public ulong Id { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? Degree { get; set; }
    public string? Major { get; set; }
    public ushort StartYear { get; set; }
    public ushort? EndYear { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Create education request
/// </summary>
public class CreateEducationDto
{
    [Required(ErrorMessage = "School name is required")]
    [MaxLength(200, ErrorMessage = "School name cannot exceed 200 characters")]
    public string SchoolName { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Degree cannot exceed 100 characters")]
    public string? Degree { get; set; }

    [MaxLength(100, ErrorMessage = "Major cannot exceed 100 characters")]
    public string? Major { get; set; }

    [Required(ErrorMessage = "Start year is required")]
    [Range(1950, 2100, ErrorMessage = "Start year must be between 1950 and 2100")]
    public ushort StartYear { get; set; }

    [Range(1950, 2100, ErrorMessage = "End year must be between 1950 and 2100")]
    public ushort? EndYear { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}

/// <summary>
/// Update education request
/// </summary>
public class UpdateEducationDto
{
    [Required(ErrorMessage = "School name is required")]
    [MaxLength(200, ErrorMessage = "School name cannot exceed 200 characters")]
    public string SchoolName { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Degree cannot exceed 100 characters")]
    public string? Degree { get; set; }

    [MaxLength(100, ErrorMessage = "Major cannot exceed 100 characters")]
    public string? Major { get; set; }

    [Required(ErrorMessage = "Start year is required")]
    [Range(1950, 2100, ErrorMessage = "Start year must be between 1950 and 2100")]
    public ushort StartYear { get; set; }

    [Range(1950, 2100, ErrorMessage = "End year must be between 1950 and 2100")]
    public ushort? EndYear { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}
