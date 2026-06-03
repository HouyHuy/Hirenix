using System.ComponentModel.DataAnnotations;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Company;

/// <summary>
/// DTO for updating company information
/// </summary>
public class UpdateCompanyDto
{
    [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    public string? Name { get; set; }

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Invalid website URL")]
    [StringLength(500, ErrorMessage = "Website URL cannot exceed 500 characters")]
    public string? Website { get; set; }

    public CompanySize? Size { get; set; }

    public uint? IndustryId { get; set; }

    public uint? CityId { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
}
