using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Application;

public class UpdateApplicationStatusDto
{
    public ApplicationStatus Status { get; set; }
    public string? ReviewNotes { get; set; }
}
