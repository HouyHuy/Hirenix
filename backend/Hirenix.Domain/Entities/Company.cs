using Hirenix.Domain.Enums;

namespace Hirenix.Domain.Entities;

public class Company
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public CompanySize? Size { get; set; }
    public uint? IndustryId { get; set; }
    public uint? CityId { get; set; }
    public string? Address { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Industry? Industry { get; set; }
    public Location? City { get; set; }
    public ICollection<EmployerProfile> EmployerProfiles { get; set; } = new List<EmployerProfile>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}

public class EmployerProfile
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong CompanyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
    public Company? Company { get; set; }
}
