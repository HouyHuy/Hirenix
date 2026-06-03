using Hirenix.Domain.Entities;
using Hirenix.Domain.Enums;
using AppEntity = Hirenix.Domain.Entities.Application;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Repository interface for application operations
/// </summary>
public interface IApplicationRepository
{
    /// <summary>
    /// Create a new job application
    /// </summary>
    Task<AppEntity> CreateApplicationAsync(AppEntity application);

    /// <summary>
    /// Get application by ID
    /// </summary>
    Task<AppEntity?> GetApplicationByIdAsync(ulong id);

    /// <summary>
    /// Get all applications for a specific candidate
    /// </summary>
    Task<List<AppEntity>> GetCandidateApplicationsAsync(ulong candidateId);

    /// <summary>
    /// Check if a candidate has already applied to a job
    /// </summary>
    Task<bool> HasAppliedAsync(ulong jobId, ulong candidateId);

    /// <summary>
    /// Get application by job and candidate
    /// </summary>
    Task<AppEntity?> GetApplicationByJobAndCandidateAsync(ulong jobId, ulong candidateId);

    /// <summary>
    /// Update application status
    /// </summary>
    Task UpdateApplicationAsync(AppEntity application);

    /// <summary>
    /// Delete/withdraw application
    /// </summary>
    Task DeleteApplicationAsync(AppEntity application);

    /// <summary>
    /// Get applications count for a specific job
    /// </summary>
    Task<int> GetJobApplicationsCountAsync(ulong jobId);

    /// <summary>
    /// Get applications that belong to jobs owned by the employer
    /// </summary>
    Task<List<AppEntity>> GetApplicationsForEmployerAsync(
        ulong employerUserId,
        ulong? jobId = null,
        ApplicationStatus? status = null);

    /// <summary>
    /// Get a specific application with ownership verification for employer
    /// </summary>
    Task<AppEntity?> GetApplicationWithDetailsForEmployerAsync(ulong employerUserId, ulong applicationId);

    /// <summary>
    /// Get applications statistics grouped by status for employer-owned jobs
    /// </summary>
    Task<Dictionary<ApplicationStatus, int>> GetApplicationsStatsByEmployerAsync(ulong employerUserId);

    /// <summary>
    /// Get applications count by job for employer-owned jobs
    /// </summary>
    Task<Dictionary<ulong, int>> GetApplicationsCountByJobAsync(ulong employerUserId);
}
