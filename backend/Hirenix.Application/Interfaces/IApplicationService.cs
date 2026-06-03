using AppEntity = Hirenix.Domain.Entities.Application;

namespace Hirenix.Application.Interfaces;

/// <summary>
/// Service interface for job application business logic
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Submit a job application with CV upload
    /// </summary>
    /// <param name="jobId">Job ID to apply to</param>
    /// <param name="candidateId">Candidate ID</param>
    /// <param name="cvStream">CV file stream</param>
    /// <param name="cvFileName">CV file name</param>
    /// <param name="coverLetter">Optional cover letter</param>
    /// <returns>Created application</returns>
    Task<AppEntity> SubmitApplicationAsync(ulong jobId, ulong candidateId, Stream cvStream, string cvFileName, string? coverLetter);

    /// <summary>
    /// Get all applications for a candidate
    /// </summary>
    Task<List<AppEntity>> GetMyApplicationsAsync(ulong candidateId);

    /// <summary>
    /// Get application details by ID
    /// </summary>
    Task<AppEntity> GetApplicationByIdAsync(ulong id, ulong candidateId);

    /// <summary>
    /// Withdraw an application
    /// </summary>
    Task WithdrawApplicationAsync(ulong id, ulong candidateId);
}
