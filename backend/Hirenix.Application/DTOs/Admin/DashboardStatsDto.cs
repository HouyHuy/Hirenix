namespace Hirenix.Application.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalJobs { get; set; }
    public long TotalApplications { get; set; }
    public int TotalCompanies { get; set; }
    public int NewUsersToday { get; set; }
    public int NewJobsToday { get; set; }
    public int NewApplicationsToday { get; set; }
    public int ActiveJobsCount { get; set; }
    public int PendingJobsCount { get; set; }
    public UsersByRoleDto UsersByRole { get; set; } = new();
}

public class UsersByRoleDto
{
    public int Candidate { get; set; }
    public int Employer { get; set; }
}
