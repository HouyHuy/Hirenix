using Hirenix.Domain.Enums;

namespace Hirenix.Domain.Entities;

/// <summary>
/// Represents a job application submitted by a candidate
/// </summary>
public class Application
{
    /// <summary>
    /// Unique identifier for the application
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// ID of the job being applied to
    /// </summary>
    public ulong JobId { get; set; }

    /// <summary>
    /// Navigation property to the job
    /// </summary>
    public Job Job { get; set; } = null!;

    /// <summary>
    /// ID of the candidate applying
    /// </summary>
    public ulong CandidateId { get; set; }

    /// <summary>
    /// Navigation property to the candidate
    /// </summary>
    public User Candidate { get; set; } = null!;

    /// <summary>
    /// URL/path to the uploaded CV file
    /// </summary>
    public string CvUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional cover letter from the candidate
    /// </summary>
    public string? CoverLetter { get; set; }

    /// <summary>
    /// Current status of the application
    /// </summary>
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    /// <summary>
    /// Date and time when the application was submitted
    /// </summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the application was last reviewed/updated by employer
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// ID of the employer who reviewed the application (if any)
    /// </summary>
    public ulong? ReviewedBy { get; set; }

    /// <summary>
    /// Navigation property to the employer who reviewed
    /// </summary>
    public User? Reviewer { get; set; }

    /// <summary>
    /// Optional notes from the employer about the application
    /// </summary>
    public string? ReviewNotes { get; set; }
}
