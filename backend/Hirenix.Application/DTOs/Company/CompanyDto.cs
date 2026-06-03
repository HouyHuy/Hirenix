using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Company;

/// <summary>
/// DTO for returning company information
/// </summary>
public class CompanyDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public CompanySize? Size { get; set; }
    public uint? IndustryId { get; set; }
    public string? IndustryName { get; set; }
    public uint? CityId { get; set; }
    public string? CityName { get; set; }
    public string? Address { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public int ActiveJobCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
