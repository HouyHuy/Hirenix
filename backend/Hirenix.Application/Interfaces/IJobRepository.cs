using Hirenix.Application.DTOs.Job;
using Hirenix.Domain.Entities;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.Interfaces;

public interface IJobRepository
{
    // ════════════════════════════════════════════════════════════════
    //  PUBLIC JOB LISTING (for candidates)
    // ════════════════════════════════════════════════════════════════
    Task<(List<Job> Jobs, int TotalCount)> GetJobsAsync(JobFilterDto filter);
    Task<Job?> GetJobByIdAsync(ulong id);
    Task<Job?> GetJobDetailAsync(ulong jobId);
    Task IncrementViewCountAsync(ulong jobId);
    Task<bool> HasUserAppliedAsync(ulong jobId, ulong userId);
    
    // ════════════════════════════════════════════════════════════════
    //  EMPLOYER JOB MANAGEMENT
    // ════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Create a new job posting
    /// </summary>
    Task<Job> CreateJobAsync(Job job);
    
    /// <summary>
    /// Update an existing job
    /// </summary>
    Task UpdateJobAsync(Job job);
    
    /// <summary>
    /// Get all jobs posted by a specific company
    /// </summary>
    Task<List<Job>> GetJobsByCompanyIdAsync(ulong companyId, JobStatus? status = null);
    
    /// <summary>
    /// Get job by ID with company ownership verification
    /// </summary>
    Task<Job?> GetJobByIdForEmployerAsync(ulong jobId, ulong companyId);
    
    /// <summary>
    /// Check if a company owns a specific job
    /// </summary>
    Task<bool> IsJobOwnedByCompanyAsync(ulong jobId, ulong companyId);
    
    /// <summary>
    /// Close a job (change status to Closed)
    /// </summary>
    Task<bool> CloseJobAsync(ulong jobId);
    
    /// <summary>
    /// Soft delete a job
    /// </summary>
    Task<bool> DeleteJobAsync(ulong jobId);
    
    /// <summary>
    /// Get application count for a job
    /// </summary>
    Task<int> GetApplicationCountAsync(ulong jobId);
    
    /// <summary>
    /// Add skills to a job
    /// </summary>
    Task AddJobSkillsAsync(ulong jobId, List<uint> skillIds);
    
    /// <summary>
    /// Remove all skills from a job (for update)
    /// </summary>
    Task RemoveJobSkillsAsync(ulong jobId);
}
