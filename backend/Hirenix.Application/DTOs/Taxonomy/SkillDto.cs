namespace Hirenix.Application.DTOs.Taxonomy;

public class SkillDto
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Category { get; set; }
}
