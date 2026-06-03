using System.ComponentModel.DataAnnotations;

namespace Hirenix.Application.DTOs.EmployerProfile;

/// <summary>
/// DTO for updating employer profile
/// </summary>
public class UpdateEmployerProfileDto
{
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string? FullName { get; set; }

    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string? Title { get; set; }

    public bool? IsAdmin { get; set; }
}
