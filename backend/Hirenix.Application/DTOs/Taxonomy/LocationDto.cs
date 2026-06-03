namespace Hirenix.Application.DTOs.Taxonomy;

public class LocationDto
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "VN";
}
