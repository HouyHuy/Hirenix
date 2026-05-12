using Hirenix.Application.DTOs.Admin;

namespace Hirenix.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<AnalyticsDto> GetAnalyticsAsync(string period);
    Task<IReadOnlyList<RecentActivityDto>> GetRecentActivitiesAsync(int limit);
}
