using Hirenix.Application.DTOs.Admin;
using Hirenix.Application.Interfaces;

namespace Hirenix.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);
        return await _adminRepository.GetDashboardStatsAsync(todayStart, todayEnd);
    }

    public async Task<AnalyticsDto> GetAnalyticsAsync(string period)
    {
        var (startDate, endDate, normalizedPeriod) = ResolvePeriod(period);
        var userGrowth = await _adminRepository.GetUsersGrowthAsync(startDate, endDate);
        var jobGrowth = await _adminRepository.GetJobsGrowthAsync(startDate, endDate);

        var dates = BuildDateSeries(startDate, endDate);
        var usersSeries = BuildSeries(dates, userGrowth);
        var jobsSeries = BuildSeries(dates, jobGrowth);
        var applicationsSeries = dates
            .Select(date => new DataPointDto { Date = date, Count = 0 })
            .ToList();

        return new AnalyticsDto
        {
            Period = normalizedPeriod,
            UsersGrowth = usersSeries,
            JobsGrowth = jobsSeries,
            ApplicationsGrowth = applicationsSeries
        };
    }

    public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivitiesAsync(int limit)
    {
        return await _adminRepository.GetRecentActivitiesAsync(limit);
    }

    private static (DateTime Start, DateTime End, string Normalized) ResolvePeriod(string period)
    {
        var normalized = (period ?? string.Empty).Trim().ToLowerInvariant();
        var days = normalized switch
        {
            "7d" => 7,
            "30d" => 30,
            "90d" => 90,
            "1y" => 365,
            _ => 30
        };

        var endDate = DateTime.UtcNow.Date.AddDays(1);
        var startDate = endDate.AddDays(-days);
        return (startDate, endDate, normalized == string.Empty ? "30d" : normalized);
    }

    private static List<string> BuildDateSeries(DateTime startDate, DateTime endDate)
    {
        var dates = new List<string>();
        var current = startDate.Date;
        while (current < endDate.Date)
        {
            dates.Add(current.ToString("yyyy-MM-dd"));
            current = current.AddDays(1);
        }
        return dates;
    }

    private static List<DataPointDto> BuildSeries(
        IEnumerable<string> dates,
        IReadOnlyDictionary<string, int> values)
    {
        return dates
            .Select(date => new DataPointDto
            {
                Date = date,
                Count = values.TryGetValue(date, out var count) ? count : 0
            })
            .ToList();
    }
}
