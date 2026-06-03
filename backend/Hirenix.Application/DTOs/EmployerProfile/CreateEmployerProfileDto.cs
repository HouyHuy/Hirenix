using System.ComponentModel.DataAnnotations;

namespace Hirenix.Application.DTOs.EmployerProfile;

/// <summary>
/// DTO for creating a new employer profile
/// </summary>
public class CreateEmployerProfileDto
{
    [Required(ErrorMessage = "Company ID is required")]
    public ulong CompanyId { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string? Title { get; set; }

    public bool IsAdmin { get; set; } = false;
}
