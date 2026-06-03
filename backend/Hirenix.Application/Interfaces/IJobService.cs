using Hirenix.Application.DTOs.Job;
using Hirenix.Application.DTOs.Common;

namespace Hirenix.Application.Interfaces;

public interface IJobService
{
    Task<ApiResponse<PaginatedResultDto<JobListItemDto>>> GetJobsAsync(JobFilterDto filter);
    Task<ApiResponse<JobListItemDto>> GetJobByIdAsync(ulong id);
    Task<ApiResponse<FilterOptionsDto>> GetFilterOptionsAsync();
    Task<ApiResponse<JobDetailDto>> GetJobDetailAsync(ulong jobId, ulong? userId = null);
}
