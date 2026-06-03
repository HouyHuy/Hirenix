using Hirenix.Application.DTOs.Job;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Service interface for employer job management operations
/// Handles job creation, updates, and management for employers
/// </summary>
public interface IEmployerJobService
{
    /// <summary>
    /// Create a new job posting
    /// Validates employer profile, expiry date, and skills
    /// </summary>
    /// <param name="userId">User ID of the employer</param>
    /// <param name="dto">Job creation data</param>
    /// <returns>Created job with employer-specific information</returns>
    Task<EmployerJobDto> CreateJobAsync(ulong userId, CreateJobDto dto);

    /// <summary>
    /// Get all jobs posted by the employer's company
    /// </summary>
    /// <param name="userId">User ID of the employer</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of jobs with employer-specific information</returns>
    Task<List<EmployerJobDto>> GetMyJobsAsync(ulong userId, ulong? companyId = null, JobStatus? status = null);

    /// <summary>
    /// Get a specific job by ID with ownership verification
    /// </summary>
    /// <param name="userId">User ID of the employer</param>
    /// <param name="jobId">Job ID</param>
    /// <returns>Job details or null if not found/not owned</returns>
    Task<EmployerJobDto?> GetJobByIdAsync(ulong userId, ulong jobId);

    /// <summary>
    /// Update an existing job
    /// Validates ownership and business rules
    /// </summary>
    /// <param name="userId">User ID of the employer</param>
    /// <param name="jobId">Job ID to update</param>
    /// <param name="dto">Update data</param>
    /// <returns>Updated job or null if not found/not owned</returns>
    Task<EmployerJobDto?> UpdateJobAsync(ulong userId, ulong jobId, UpdateJobDto dto);

    /// <summary>
    /// Close a job (change status to Closed)
    /// Prevents new applications
    /// </summary>
    /// <param name="userId">User ID of the employer</param>
    /// <param name="jobId">Job ID to close</param>
    /// <returns>True if closed successfully</returns>
    Task<bool> CloseJobAsync(ulong userId, ulong jobId);

    /// <summary>
    /// Delete a job (soft delete)
    /// </summary>
    /// <param name="userId">User ID of the employer</param>
    /// <param name="jobId">Job ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteJobAsync(ulong userId, ulong jobId);
}
