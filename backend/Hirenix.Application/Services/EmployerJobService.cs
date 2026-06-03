using Hirenix.Application.DTOs.Job;
using Hirenix.Application.DTOs.Taxonomy;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Domain.Enums;

namespace Hirenix.Application.Services;

/// <summary>
/// Service implementation for employer job management
/// Handles job creation, updates, and management with business logic validation
/// </summary>
public class EmployerJobService : IEmployerJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IEmployerProfileRepository _employerProfileRepository;

    public EmployerJobService(
        IJobRepository jobRepository,
        IEmployerProfileRepository employerProfileRepository)
    {
        _jobRepository = jobRepository;
        _employerProfileRepository = employerProfileRepository;
    }

    public async Task<EmployerJobDto> CreateJobAsync(ulong userId, CreateJobDto dto)
    {
        // 1. Verify user has employer profile
        var employerProfile = await _employerProfileRepository.GetByUserIdAsync(userId);
        if (employerProfile == null)
        {
            throw new InvalidOperationException("Please create an employer profile first");
        }

        if (employerProfile.CompanyId == 0)
        {
            throw new InvalidOperationException("Employer profile must be linked to a company");
        }

        var companyId = employerProfile.CompanyId;

        // 2. Validate expiry date
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dto.ExpiryDate <= today)
        {
            throw new ArgumentException("Expiry date must be in the future");
        }

        // Validate expiry date is not too far (max 90 days)
        var maxExpiryDate = today.AddDays(90);
        if (dto.ExpiryDate > maxExpiryDate)
        {
            throw new ArgumentException("Expiry date cannot be more than 90 days in the future");
        }

        // 3. Validate salary range
        if (dto.SalaryMin.HasValue && dto.SalaryMax.HasValue)
        {
            if (dto.SalaryMin.Value > dto.SalaryMax.Value)
            {
                throw new ArgumentException("Minimum salary cannot be greater than maximum salary");
            }
        }

        // 4. Create job entity
        var job = new Job
        {
            Title = dto.Title,
            Description = dto.Description,
            Requirements = dto.Requirements,
            Benefits = dto.Responsibilities, // Map Responsibilities to Benefits
            CompanyId = companyId,
            IndustryId = dto.IndustryId,
            CityId = dto.LocationId,
            WorkType = dto.WorkType,
            Level = dto.Level,
            SalaryMin = dto.SalaryMin.HasValue ? (long)dto.SalaryMin.Value : null,
            SalaryMax = dto.SalaryMax.HasValue ? (long)dto.SalaryMax.Value : null,
            Deadline = dto.ExpiryDate,
            Status = JobStatus.Active,
            CreatedBy = employerProfile.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ViewsCount = 0
        };

        // 5. Save job
        var createdJob = await _jobRepository.CreateJobAsync(job);

        // 6. Add skills if provided
        if (dto.SkillIds != null && dto.SkillIds.Any())
        {
            await _jobRepository.AddJobSkillsAsync(createdJob.Id, dto.SkillIds);
        }

        // 7. Get full job details and return
        var jobDetail = await _jobRepository.GetJobByIdForEmployerAsync(createdJob.Id, companyId);
        return await MapToEmployerJobDto(jobDetail!);
    }

    public async Task<List<EmployerJobDto>> GetMyJobsAsync(ulong userId, ulong? companyId = null, JobStatus? status = null)
    {
        // 1. Get employer profile
        var employerProfile = await _employerProfileRepository.GetByUserIdAsync(userId);
        if (employerProfile == null)
        {
            throw new InvalidOperationException("Employer profile not found");
        }

        if (employerProfile.CompanyId == 0)
        {
            return new List<EmployerJobDto>();
        }

        if (companyId.HasValue && companyId.Value != employerProfile.CompanyId)
        {
            throw new UnauthorizedAccessException("You don't have permission to view jobs for this company");
        }

        // 2. Get jobs by company
        var jobs = await _jobRepository.GetJobsByCompanyIdAsync(employerProfile.CompanyId, status);

        // 3. Map to DTOs with employer-specific info
        var jobDtos = new List<EmployerJobDto>();
        foreach (var job in jobs)
        {
            var dto = await MapToEmployerJobDto(job);
            jobDtos.Add(dto);
        }

        return jobDtos;
    }

    public async Task<EmployerJobDto?> GetJobByIdAsync(ulong userId, ulong jobId)
    {
        // 1. Get employer profile
        var employerProfile = await _employerProfileRepository.GetByUserIdAsync(userId);
        if (employerProfile == null || employerProfile.CompanyId == 0)
        {
            return null;
        }

        // 2. Get job with ownership verification
        var job = await _jobRepository.GetJobByIdAsync(jobId);
        if (job == null)
        {
            return null;
        }

        if (job.CompanyId != employerProfile.CompanyId)
        {
            throw new UnauthorizedAccessException("You don't have permission to view this job");
        }

        // 3. Map to DTO
        return await MapToEmployerJobDto(job);
    }

    public async Task<EmployerJobDto?> UpdateJobAsync(ulong userId, ulong jobId, UpdateJobDto dto)
    {
        // 1. Get employer profile
        var employerProfile = await _employerProfileRepository.GetByUserIdAsync(userId);
        if (employerProfile == null || employerProfile.CompanyId == 0)
        {
            throw new InvalidOperationException("Employer profile not found");
        }

        // 2. Verify ownership
        var job = await _jobRepository.GetJobByIdAsync(jobId);
        if (job == null)
        {
            return null;
        }

        if (job.CompanyId != employerProfile.CompanyId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this job");
        }

        // 3. Cannot update closed jobs
        if (job.Status == JobStatus.Closed)
        {
            throw new InvalidOperationException("Cannot update a closed job");
        }

        // 4. Update fields if provided
        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            job.Title = dto.Title;
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            job.Description = dto.Description;
        }

        if (!string.IsNullOrWhiteSpace(dto.Requirements))
        {
            job.Requirements = dto.Requirements;
        }

        if (dto.Responsibilities != null)
        {
            job.Benefits = dto.Responsibilities; // Map Responsibilities to Benefits
        }

        if (dto.IndustryId.HasValue)
        {
            job.IndustryId = dto.IndustryId.Value;
        }

        if (dto.LocationId.HasValue)
        {
            job.CityId = dto.LocationId.Value;
        }

        if (dto.WorkType.HasValue)
        {
            job.WorkType = dto.WorkType.Value;
        }

        if (dto.Level.HasValue)
        {
            job.Level = dto.Level.Value;
        }

        if (dto.SalaryMin.HasValue)
        {
            job.SalaryMin = (long)dto.SalaryMin.Value;
        }

        if (dto.SalaryMax.HasValue)
        {
            job.SalaryMax = (long)dto.SalaryMax.Value;
        }

        // Validate salary range
        if (job.SalaryMin.HasValue && job.SalaryMax.HasValue)
        {
            if (job.SalaryMin.Value > job.SalaryMax.Value)
            {
                throw new ArgumentException("Minimum salary cannot be greater than maximum salary");
            }
        }

        if (dto.ExpiryDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dto.ExpiryDate.Value <= today)
            {
                throw new ArgumentException("Expiry date must be in the future");
            }
            job.Deadline = dto.ExpiryDate.Value;
        }

        // 5. Update skills if provided
        if (dto.SkillIds != null)
        {
            // Remove old skills and add new ones
            await _jobRepository.RemoveJobSkillsAsync(jobId);
            if (dto.SkillIds.Any())
            {
                await _jobRepository.AddJobSkillsAsync(jobId, dto.SkillIds);
            }
        }

        // 6. Update timestamp
        job.UpdatedAt = DateTime.UtcNow;

        // 7. Save changes
        await _jobRepository.UpdateJobAsync(job);

        // 8. Get updated job and return
        var updatedJob = await _jobRepository.GetJobByIdForEmployerAsync(jobId, employerProfile.CompanyId);
        return await MapToEmployerJobDto(updatedJob!);
    }

    public async Task<bool> CloseJobAsync(ulong userId, ulong jobId)
    {
        // 1. Get employer profile
        var employerProfile = await _employerProfileRepository.GetByUserIdAsync(userId);
        if (employerProfile == null || employerProfile.CompanyId == 0)
        {
            return false;
        }

        // 2. Verify ownership
        var job = await _jobRepository.GetJobByIdAsync(jobId);
        if (job == null)
        {
            return false;
        }

        if (job.CompanyId != employerProfile.CompanyId)
        {
            throw new UnauthorizedAccessException("You don't have permission to close this job");
        }

        if (job.Status == JobStatus.Closed)
        {
            throw new InvalidOperationException("Job is already closed");
        }

        // 3. Close job
        return await _jobRepository.CloseJobAsync(jobId);
    }

    public async Task<bool> DeleteJobAsync(ulong userId, ulong jobId)
    {
        // 1. Get employer profile
        var employerProfile = await _employerProfileRepository.GetByUserIdAsync(userId);
        if (employerProfile == null || employerProfile.CompanyId == 0)
        {
            return false;
        }

        // 2. Verify ownership
        var job = await _jobRepository.GetJobByIdAsync(jobId);
        if (job == null)
        {
            return false;
        }

        if (job.CompanyId != employerProfile.CompanyId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this job");
        }

        // 3. Check if job has applications
        var applicationsCount = await _jobRepository.GetApplicationCountAsync(jobId);
        if (applicationsCount > 0)
        {
            throw new InvalidOperationException("Cannot delete job with existing applications");
        }

        // 4. Delete job (soft delete)
        return await _jobRepository.DeleteJobAsync(jobId);
    }

    // ════════════════════════════════════════════════════════════════
    //  PRIVATE HELPER METHODS
    // ════════════════════════════════════════════════════════════════

    private async Task<EmployerJobDto> MapToEmployerJobDto(Job job)
    {
        // Manual mapping from Job to EmployerJobDto
        var dto = new EmployerJobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Requirements = job.Requirements,
            Benefits = job.Benefits,
            WorkType = job.WorkType,
            Level = job.Level,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            IsSalaryVisible = job.IsSalaryVisible,
            Deadline = job.Deadline,
            Status = job.Status,
            IsFeatured = job.IsFeatured,
            ViewsCount = job.ViewsCount,
            ApplicationsCount = job.ApplicationsCount,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            
            // Company info
            Company = new CompanyInfoDto
            {
                Id = job.CompanyId,
                Name = job.Company?.Name ?? string.Empty,
                LogoUrl = job.Company?.LogoUrl,
                Website = job.Company?.Website,
                Description = job.Company?.Description
            },
            
            // Location
            City = job.City?.Name,
            
            // Industry
            Industry = job.Industry?.Name,
            
            // Skills
            Skills = job.Skills?.Select(js => new JobSkillDto
            {
                Id = js.SkillId,
                Name = js.Skill?.Name ?? string.Empty,
                IsRequired = js.IsRequired
            }).ToList() ?? new List<JobSkillDto>(),
            
            // Employer-specific fields
            ApplicationCount = await _jobRepository.GetApplicationCountAsync(job.Id),
            ViewCount = (int)job.ViewsCount,
            CanEdit = job.Status == JobStatus.Active && job.Deadline >= DateOnly.FromDateTime(DateTime.UtcNow),
            CanClose = job.Status == JobStatus.Active
        };

        return dto;
    }
}
