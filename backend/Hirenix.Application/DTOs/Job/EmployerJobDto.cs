namespace Hirenix.Application.DTOs.Job;

/// <summary>
/// Extended job DTO with employer-specific information
/// Includes application count, view count, and management permissions
/// </summary>
public class EmployerJobDto : JobDetailDto
{
    /// <summary>
    /// Number of applications received for this job
    /// </summary>
    public int ApplicationCount { get; set; }

    /// <summary>
    /// Number of times this job has been viewed
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Whether the current user can edit this job
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Whether the current user can close this job
    /// </summary>
    public bool CanClose { get; set; }
}
