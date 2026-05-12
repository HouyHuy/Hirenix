using System.ComponentModel.DataAnnotations;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Candidate;

/// <summary>
/// Add skills to candidate profile
/// </summary>
public class AddSkillsDto
{
    [Required(ErrorMessage = "At least one skill is required")]
    [MinLength(1, ErrorMessage = "At least one skill is required")]
    public List<SkillWithLevelDto> Skills { get; set; } = new();
}

/// <summary>
/// Skill with optional proficiency level
/// </summary>
public class SkillWithLevelDto
{
    [Required(ErrorMessage = "Skill ID is required")]
    public uint SkillId { get; set; }

    public SkillLevel? Level { get; set; }
}
