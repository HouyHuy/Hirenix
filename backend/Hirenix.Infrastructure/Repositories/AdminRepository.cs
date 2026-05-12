using Hirenix.Application.DTOs.Admin;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Enums;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly HirenixDbContext _context;

    public AdminRepository(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime todayStart, DateTime todayEnd)
    {
        // Execute queries sequentially to avoid DbContext concurrency issues
        var totalUsers = await _context.Users.AsNoTracking().CountAsync();
        var totalJobs = await _context.Jobs.AsNoTracking().CountAsync();
        var totalCompanies = await _context.Companies.AsNoTracking().CountAsync();

        var totalApplications = await _context.Jobs.AsNoTracking()
            .SumAsync(j => (long)j.ApplicationsCount);

        var newUsersToday = await _context.Users.AsNoTracking()
            .CountAsync(u => u.CreatedAt >= todayStart && u.CreatedAt < todayEnd);
        var newJobsToday = await _context.Jobs.AsNoTracking()
            .CountAsync(j => j.CreatedAt >= todayStart && j.CreatedAt < todayEnd);

        var activeJobsCount = await _context.Jobs.AsNoTracking()
            .CountAsync(j => j.Status == JobStatus.Active);
        var pendingJobsCount = await _context.Jobs.AsNoTracking()
            .CountAsync(j => j.Status == JobStatus.Pending);

        var roleCounts = await _context.Users.AsNoTracking()
            .GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToListAsync();

        var candidateCount = roleCounts.FirstOrDefault(r => r.Role == UserRole.Candidate)?.Count ?? 0;
        var employerCount = roleCounts.FirstOrDefault(r => r.Role == UserRole.Employer)?.Count ?? 0;

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalJobs = totalJobs,
            TotalCompanies = totalCompanies,
            TotalApplications = totalApplications,
            NewUsersToday = newUsersToday,
            NewJobsToday = newJobsToday,
            NewApplicationsToday = 0,
            ActiveJobsCount = activeJobsCount,
            PendingJobsCount = pendingJobsCount,
            UsersByRole = new UsersByRoleDto
            {
                Candidate = candidateCount,
                Employer = employerCount
            }
        };
    }

    public async Task<Dictionary<string, int>> GetUsersGrowthAsync(DateTime startDate, DateTime endDate)
    {
        var rows = await _context.Users.AsNoTracking()
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt < endDate)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.ToDictionary(
            row => row.Date.ToString("yyyy-MM-dd"),
            row => row.Count
        );
    }

    public async Task<Dictionary<string, int>> GetJobsGrowthAsync(DateTime startDate, DateTime endDate)
    {
        var rows = await _context.Jobs.AsNoTracking()
            .Where(j => j.CreatedAt >= startDate && j.CreatedAt < endDate)
            .GroupBy(j => j.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.ToDictionary(
            row => row.Date.ToString("yyyy-MM-dd"),
            row => row.Count
        );
    }

    public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivitiesAsync(int limit)
    {
        var userActivities = await _context.Users.AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(limit)
            .Select(u => new RecentActivityDto
            {
                Id = (long)u.Id,
                Type = "user_registered",
                Description = "New user registered",
                Timestamp = u.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UserId = u.Id,
                UserName = !string.IsNullOrEmpty(u.Email) ? u.Email : u.Phone
            })
            .ToListAsync();

        var jobActivities = await _context.Jobs.AsNoTracking()
            .Include(j => j.Company)
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .Select(j => new RecentActivityDto
            {
                Id = (long)j.Id,
                Type = "job_posted",
                Description = "New job posted",
                Timestamp = j.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                JobId = j.Id,
                JobTitle = j.Title,
                CompanyName = j.Company != null ? j.Company.Name : null
            })
            .ToListAsync();

        var companyActivities = await _context.Companies.AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Take(limit)
            .Select(c => new RecentActivityDto
            {
                Id = (long)c.Id,
                Type = "company_created",
                Description = "New company created",
                Timestamp = c.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                CompanyName = c.Name
            })
            .ToListAsync();

        return userActivities
            .Concat(jobActivities)
            .Concat(companyActivities)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToList();
    }
}
