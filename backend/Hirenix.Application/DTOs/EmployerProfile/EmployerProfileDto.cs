namespace Hirenix.Application.DTOs.EmployerProfile;

/// <summary>
/// DTO for returning employer profile information
/// </summary>
public class EmployerProfileDto
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public ulong CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
