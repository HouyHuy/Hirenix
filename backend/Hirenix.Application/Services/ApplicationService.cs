using Hirenix.Application.Interfaces;
using Hirenix.Domain.Enums;
using AppEntity = Hirenix.Domain.Entities.Application;

namespace Hirenix.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IFileStorageService _fileStorageService;

    public ApplicationService(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IFileStorageService fileStorageService)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<AppEntity> SubmitApplicationAsync(ulong jobId, ulong candidateId, Stream cvStream, string cvFileName, string? coverLetter)
    {
        // 1. Check if job exists and is active
        var job = await _jobRepository.GetJobByIdAsync(jobId);
        if (job == null)
        {
            throw new InvalidOperationException("Job not found.");
        }

        if (job.Status != JobStatus.Active)
        {
            throw new InvalidOperationException("This job is no longer accepting applications.");
        }

        // 2. Check if deadline has passed
        if (job.Deadline != null && job.Deadline < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new InvalidOperationException("The application deadline for this job has passed.");
        }

        // 3. Check if candidate has already applied
        var hasApplied = await _applicationRepository.HasAppliedAsync(jobId, candidateId);
        if (hasApplied)
        {
            throw new InvalidOperationException("You have already applied to this job.");
        }

        // 4. Upload CV file
        string cvUrl;
        try
        {
            cvUrl = await _fileStorageService.SaveFileAsync(cvStream, cvFileName, "cvs");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload CV: {ex.Message}");
        }

        // 5. Create application
        var application = new AppEntity
        {
            JobId = jobId,
            CandidateId = candidateId,
            CvUrl = cvUrl,
            CoverLetter = coverLetter,
            Status = ApplicationStatus.Applied,
            AppliedAt = DateTime.UtcNow
        };

        var createdApplication = await _applicationRepository.CreateApplicationAsync(application);

        // 6. Increment job applications count
        job.ApplicationsCount++;
        await _jobRepository.UpdateJobAsync(job);

        return createdApplication;
    }

    public async Task<List<AppEntity>> GetMyApplicationsAsync(ulong candidateId)
    {
        return await _applicationRepository.GetCandidateApplicationsAsync(candidateId);
    }

    public async Task<AppEntity> GetApplicationByIdAsync(ulong id, ulong candidateId)
    {
        var application = await _applicationRepository.GetApplicationByIdAsync(id);
        
        if (application == null)
        {
            throw new InvalidOperationException("Application not found.");
        }

        // Ensure candidate can only view their own applications
        if (application.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this application.");
        }

        return application;
    }

    public async Task WithdrawApplicationAsync(ulong id, ulong candidateId)
    {
        var application = await _applicationRepository.GetApplicationByIdAsync(id);
        
        if (application == null)
        {
            throw new InvalidOperationException("Application not found.");
        }

        // Ensure candidate can only withdraw their own applications
        if (application.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException("You do not have permission to withdraw this application.");
        }

        // Check if application can be withdrawn
        if (application.Status == ApplicationStatus.Withdrawn)
        {
            throw new InvalidOperationException("This application has already been withdrawn.");
        }

        if (application.Status == ApplicationStatus.Accepted)
        {
            throw new InvalidOperationException("Cannot withdraw an accepted application.");
        }

        // Delete CV file
        try
        {
            await _fileStorageService.DeleteFileAsync(application.CvUrl);
        }
        catch
        {
            // Log error but continue - file deletion is not critical
        }

        // Delete application
        await _applicationRepository.DeleteApplicationAsync(application);

        // Decrement job applications count
        var job = await _jobRepository.GetJobByIdAsync(application.JobId);
        if (job != null && job.ApplicationsCount > 0)
        {
            job.ApplicationsCount--;
            await _jobRepository.UpdateJobAsync(job);
        }
    }
}
