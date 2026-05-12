using Hirenix.Application.DTOs.Admin;

namespace Hirenix.Application.Interfaces;

public interface IAdminRepository
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime todayStart, DateTime todayEnd);
    Task<Dictionary<string, int>> GetUsersGrowthAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<string, int>> GetJobsGrowthAsync(DateTime startDate, DateTime endDate);
    Task<IReadOnlyList<RecentActivityDto>> GetRecentActivitiesAsync(int limit);
}
