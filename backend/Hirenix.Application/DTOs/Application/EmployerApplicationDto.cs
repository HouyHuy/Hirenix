using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Application;

public class EmployerApplicationDto
{
    public ulong Id { get; set; }
    public ulong JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public ulong CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidatePhone { get; set; }
    public string? CandidatePhotoUrl { get; set; }
    public string CvUrl { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewNotes { get; set; }
    public int YearsOfExperience { get; set; }
    public List<string> Skills { get; set; } = new();
    public string? CurrentPosition { get; set; }
}
