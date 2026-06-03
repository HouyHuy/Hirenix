namespace Hirenix.Domain.Enums;

/// <summary>
/// Status of a job application
/// </summary>
public enum ApplicationStatus
{
    /// <summary>
    /// Application has been submitted and is pending review
    /// </summary>
    Applied = 0,

    /// <summary>
    /// Application is currently being reviewed by employer
    /// </summary>
    Reviewing = 1,

    /// <summary>
    /// Candidate has been shortlisted for interview
    /// </summary>
    Shortlisted = 2,

    /// <summary>
    /// Application has been rejected
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Candidate has been offered the position
    /// </summary>
    Accepted = 4,

    /// <summary>
    /// Application was withdrawn by the candidate
    /// </summary>
    Withdrawn = 5
}
