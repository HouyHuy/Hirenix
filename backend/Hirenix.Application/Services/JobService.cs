using Hirenix.Application.DTOs.Common;
using Hirenix.Application.DTOs.Job;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Hirenix.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IMemoryCache _cache;
    private readonly ITaxonomyRepository _taxonomyRepository;
    private const string FilterOptionsCacheKey = "job_filter_options";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public JobService(
        IJobRepository jobRepository,
        IMemoryCache cache,
        ITaxonomyRepository taxonomyRepository)
    {
        _jobRepository = jobRepository;
        _cache = cache;
        _taxonomyRepository = taxonomyRepository;
    }

    public async Task<ApiResponse<PaginatedResultDto<JobListItemDto>>> GetJobsAsync(JobFilterDto filter)
    {
        try
        {
            // Validate and sanitize filter parameters
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1 || filter.PageSize > 100) filter.PageSize = 20;

            // Get jobs from repository
            var (jobs, totalCount) = await _jobRepository.GetJobsAsync(filter);

            // Map to DTOs
            var jobDtos = jobs.Select(job => new JobListItemDto
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.Company?.Name ?? "Unknown Company",
                CompanyLogo = job.Company?.LogoUrl,
                CityName = job.City?.Name,
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
                Skills = job.Skills.Select(js => js.Skill?.Name ?? "").Where(s => !string.IsNullOrEmpty(s)).Take(5).ToList(),
                CreatedAt = job.CreatedAt
            }).ToList();

            // Calculate pagination metadata
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var result = new PaginatedResultDto<JobListItemDto>
            {
                Data = jobDtos,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages,
                    TotalItems = totalCount
                }
            };

            return ApiResponse<PaginatedResultDto<JobListItemDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedResultDto<JobListItemDto>>.Fail($"Failed to retrieve jobs: {ex.Message}");
        }
    }

    public async Task<ApiResponse<JobListItemDto>> GetJobByIdAsync(ulong id)
    {
        try
        {
            var job = await _jobRepository.GetJobByIdAsync(id);

            if (job == null)
            {
                return ApiResponse<JobListItemDto>.Fail("Job not found");
            }

            var jobDto = new JobListItemDto
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.Company?.Name ?? "Unknown Company",
                CompanyLogo = job.Company?.LogoUrl,
                CityName = job.City?.Name,
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
                Skills = job.Skills.Select(js => js.Skill?.Name ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList(),
                CreatedAt = job.CreatedAt
            };

            return ApiResponse<JobListItemDto>.Ok(jobDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<JobListItemDto>.Fail($"Failed to retrieve job: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FilterOptionsDto>> GetFilterOptionsAsync()
    {
        try
        {
            // Try to get from cache
            if (_cache.TryGetValue(FilterOptionsCacheKey, out FilterOptionsDto? cachedOptions) && cachedOptions != null)
            {
                return ApiResponse<FilterOptionsDto>.Ok(cachedOptions);
            }

            // Build filter options
            var locations = await _taxonomyRepository.GetAllLocationsAsync();
            var cities = locations.Select(l => new LocationOptionDto
            {
                Id = l.Id,
                Name = l.Name
            }).ToList();

            var industriesData = await _taxonomyRepository.GetAllIndustriesAsync();
            var industries = industriesData.Select(i => new IndustryOptionDto
            {
                Id = i.Id,
                Name = i.Name
            }).ToList();

            var workTypes = Enum.GetValues<WorkType>()
                .Select(wt => new WorkTypeOptionDto
                {
                    Value = (int)wt,
                    Name = wt.ToString()
                })
                .ToList();

            var levels = Enum.GetValues<JobLevel>()
                .Select(l => new JobLevelOptionDto
                {
                    Value = (int)l,
                    Name = l.ToString()
                })
                .ToList();

            var options = new FilterOptionsDto
            {
                Cities = cities,
                Industries = industries,
                WorkTypes = workTypes,
                Levels = levels
            };

            // Cache the options
            _cache.Set(FilterOptionsCacheKey, options, CacheDuration);

            return ApiResponse<FilterOptionsDto>.Ok(options);
        }
        catch (Exception ex)
        {
            return ApiResponse<FilterOptionsDto>.Fail($"Failed to retrieve filter options: {ex.Message}");
        }
    }

    public async Task<ApiResponse<JobDetailDto>> GetJobDetailAsync(ulong jobId, ulong? userId = null)
    {
        try
        {
            // Get job detail from repository
            var job = await _jobRepository.GetJobDetailAsync(jobId);

            if (job == null)
            {
                return ApiResponse<JobDetailDto>.Fail("Job not found or no longer available");
            }

            // Increment view count
            await _jobRepository.IncrementViewCountAsync(jobId);

            // Check if user has applied (if userId provided)
            bool hasApplied = false;
            if (userId.HasValue)
            {
                hasApplied = await _jobRepository.HasUserAppliedAsync(jobId, userId.Value);
            }

            // Map to DTO
            var jobDetailDto = new JobDetailDto
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
                Company = new CompanyInfoDto
                {
                    Id = job.Company?.Id ?? 0,
                    Name = job.Company?.Name ?? "Unknown Company",
                    LogoUrl = job.Company?.LogoUrl,
                    Website = job.Company?.Website,
                    Description = job.Company?.Description
                },
                City = job.City?.Name,
                Industry = job.Industry?.Name,
                Skills = job.Skills.Select(js => new JobSkillDto
                {
                    Id = js.Skill?.Id ?? 0,
                    Name = js.Skill?.Name ?? "",
                    IsRequired = js.IsRequired
                }).ToList(),
                HasApplied = hasApplied
            };

            return ApiResponse<JobDetailDto>.Ok(jobDetailDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<JobDetailDto>.Fail($"Failed to retrieve job detail: {ex.Message}");
        }
    }
}
