using Hirenix.Application.DTOs.Application;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.Services;

public class EmployerApplicationService : IEmployerApplicationService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICandidateProfileRepository _candidateProfileRepository;
    private readonly IFileStorageService _fileStorageService;

    public EmployerApplicationService(
        IApplicationRepository applicationRepository,
        ICandidateProfileRepository candidateProfileRepository,
        IFileStorageService fileStorageService)
    {
        _applicationRepository = applicationRepository;
        _candidateProfileRepository = candidateProfileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<List<EmployerApplicationDto>> GetApplicationsAsync(
        ulong userId,
        ulong? jobId = null,
        ApplicationStatus? status = null)
    {
        var applications = await _applicationRepository.GetApplicationsForEmployerAsync(userId, jobId, status);
        var result = new List<EmployerApplicationDto>(applications.Count);

        foreach (var application in applications)
        {
            result.Add(await MapToDtoAsync(application));
        }

        return result;
    }

    public async Task<EmployerApplicationDto?> GetApplicationByIdAsync(ulong userId, ulong applicationId)
    {
        var application = await _applicationRepository.GetApplicationWithDetailsForEmployerAsync(userId, applicationId);
        if (application == null)
        {
            return null;
        }

        return await MapToDtoAsync(application);
    }

    public async Task<bool> UpdateApplicationStatusAsync(
        ulong userId,
        ulong applicationId,
        ApplicationStatus newStatus,
        string? reviewNotes = null)
    {
        var application = await _applicationRepository.GetApplicationWithDetailsForEmployerAsync(userId, applicationId);
        if (application == null)
        {
            return false;
        }

        ValidateTransition(application.Status, newStatus);

        application.Status = newStatus;
        application.ReviewedAt = DateTime.UtcNow;
        application.ReviewedBy = userId;
        application.ReviewNotes = string.IsNullOrWhiteSpace(reviewNotes) ? null : reviewNotes.Trim();

        await _applicationRepository.UpdateApplicationAsync(application);
        return true;
    }

    public async Task<ApplicationStatisticsDto> GetStatisticsAsync(ulong userId)
    {
        var byStatus = await _applicationRepository.GetApplicationsStatsByEmployerAsync(userId);
        var byJob = await _applicationRepository.GetApplicationsCountByJobAsync(userId);

        return new ApplicationStatisticsDto
        {
            Total = byStatus.Values.Sum(),
            ByStatus = byStatus.ToDictionary(x => x.Key.ToString(), x => x.Value),
            ByJob = byJob
        };
    }

    private static void ValidateTransition(ApplicationStatus current, ApplicationStatus next)
    {
        if (current == next)
        {
            return;
        }

        if (current == ApplicationStatus.Withdrawn)
        {
            throw new InvalidOperationException("Withdrawn application cannot be updated");
        }

        if (current is ApplicationStatus.Rejected or ApplicationStatus.Accepted)
        {
            throw new InvalidOperationException("Rejected or accepted application cannot be moved to another status");
        }

        var allowed = current switch
        {
            ApplicationStatus.Applied => new[] { ApplicationStatus.Reviewing, ApplicationStatus.Shortlisted, ApplicationStatus.Rejected, ApplicationStatus.Accepted },
            ApplicationStatus.Reviewing => new[] { ApplicationStatus.Shortlisted, ApplicationStatus.Rejected, ApplicationStatus.Accepted },
            ApplicationStatus.Shortlisted => new[] { ApplicationStatus.Rejected, ApplicationStatus.Accepted },
            _ => Array.Empty<ApplicationStatus>()
        };

        if (!allowed.Contains(next))
        {
            throw new InvalidOperationException($"Invalid status transition: {current} -> {next}");
        }
    }

    private async Task<EmployerApplicationDto> MapToDtoAsync(Hirenix.Domain.Entities.Application application)
    {
        var candidateProfile = await _candidateProfileRepository.GetByUserIdAsync(application.CandidateId);
        var experiences = candidateProfile?.Experiences ?? Array.Empty<CandidateExperience>();
        var yearsOfExperience = CalculateYearsOfExperience(experiences);

        return new EmployerApplicationDto
        {
            Id = application.Id,
            JobId = application.JobId,
            JobTitle = application.Job?.Title ?? string.Empty,
            CandidateId = application.CandidateId,
            CandidateName = candidateProfile?.FullName ?? application.Candidate?.Email ?? "Unknown Candidate",
            CandidateEmail = application.Candidate?.Email ?? string.Empty,
            CandidatePhone = application.Candidate?.Phone,
            CandidatePhotoUrl = candidateProfile?.AvatarUrl,
            CvUrl = _fileStorageService.GetAccessUrl(application.CvUrl),
            CoverLetter = application.CoverLetter,
            Status = application.Status,
            AppliedDate = application.AppliedAt,
            ReviewedDate = application.ReviewedAt,
            ReviewNotes = application.ReviewNotes,
            YearsOfExperience = yearsOfExperience,
            Skills = candidateProfile?.Skills.Select(s => s.Skill?.Name ?? string.Empty).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? new List<string>(),
            CurrentPosition = experiences.OrderByDescending(e => e.IsCurrent).ThenByDescending(e => e.EndDate ?? DateOnly.MaxValue).Select(e => e.Position).FirstOrDefault()
        };
    }

    private static int CalculateYearsOfExperience(IEnumerable<CandidateExperience> experiences)
    {
        var totalDays = experiences.Sum(exp =>
        {
            var end = exp.IsCurrent ? DateOnly.FromDateTime(DateTime.UtcNow) : (exp.EndDate ?? exp.StartDate);
            var days = end.DayNumber - exp.StartDate.DayNumber;
            return Math.Max(days, 0);
        });

        return totalDays <= 0 ? 0 : (int)Math.Round(totalDays / 365d, MidpointRounding.AwayFromZero);
    }
}
