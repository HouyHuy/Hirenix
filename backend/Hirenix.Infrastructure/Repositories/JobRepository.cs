using Hirenix.Application.DTOs.Job;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Domain.Enums;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for job operations
/// </summary>
public class JobRepository : IJobRepository
{
    private readonly HirenixDbContext _context;

    public JobRepository(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Job> Jobs, int TotalCount)> GetJobsAsync(JobFilterDto filter)
    {
        var query = _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.City)
            .Include(j => j.Industry)
            .Include(j => j.Skills)
                .ThenInclude(js => js.Skill)
            .AsQueryable();

        // Filter by status (default to Active only)
        if (filter.Status.HasValue)
        {
            query = query.Where(j => j.Status == filter.Status.Value);
        }
        else
        {
            query = query.Where(j => j.Status == JobStatus.Active);
        }

        // Filter by deadline (only show jobs that haven't expired)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        query = query.Where(j => j.Deadline >= today);

        // Search filter (title or company name)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search;
            query = query.Where(j =>
                EF.Functions.Like(j.Title, $"%{searchTerm}%") ||
                (j.Company != null && EF.Functions.Like(j.Company.Name, $"%{searchTerm}%"))
            );
        }

        // City filter
        if (filter.CityId.HasValue)
        {
            query = query.Where(j => j.CityId == filter.CityId.Value);
        }

        // Industry filter
        if (filter.IndustryId.HasValue)
        {
            query = query.Where(j => j.IndustryId == filter.IndustryId.Value);
        }

        // Work type filter
        if (filter.WorkType.HasValue)
        {
            query = query.Where(j => j.WorkType == filter.WorkType.Value);
        }

        // Job level filter
        if (filter.Level.HasValue)
        {
            query = query.Where(j => j.Level == filter.Level.Value);
        }

        // Salary range filter (overlap logic)
        if (filter.MinSalary.HasValue)
        {
            query = query.Where(j => 
                j.SalaryMax == null || j.SalaryMax >= filter.MinSalary.Value
            );
        }

        if (filter.MaxSalary.HasValue)
        {
            query = query.Where(j => 
                j.SalaryMin == null || j.SalaryMin <= filter.MaxSalary.Value
            );
        }

        // Sorting
        query = filter.SortBy?.ToLower() switch
        {
            "salary" => query.OrderByDescending(j => j.SalaryMax ?? 0)
                             .ThenByDescending(j => j.SalaryMin ?? 0),
            "title" => query.OrderBy(j => j.Title),
            "deadline" => query.OrderBy(j => j.Deadline),
            _ => query.OrderByDescending(j => j.CreatedAt) // Default: newest first
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var jobs = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (jobs, totalCount);
    }

    public async Task<Job?> GetJobByIdAsync(ulong id)
    {
        return await _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.City)
            .Include(j => j.Industry)
            .Include(j => j.Creator)
            .Include(j => j.Skills)
                .ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<Job?> GetJobDetailAsync(ulong jobId)
    {
        return await _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.City)
            .Include(j => j.Industry)
            .Include(j => j.Skills)
                .ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == jobId && j.Status == JobStatus.Active);
    }

    public async Task IncrementViewCountAsync(ulong jobId)
    {
        var job = await _context.Jobs.FindAsync(jobId);
        if (job != null)
        {
            job.ViewsCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasUserAppliedAsync(ulong jobId, ulong userId)
    {
        return await _context.Applications
            .AnyAsync(a => a.JobId == jobId && a.CandidateId == userId);
    }

    public async Task UpdateJobAsync(Job job)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync();
    }

    // ════════════════════════════════════════════════════════════════
    //  EMPLOYER JOB MANAGEMENT METHODS
    // ════════════════════════════════════════════════════════════════

    public async Task<Job> CreateJobAsync(Job job)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<List<Job>> GetJobsByCompanyIdAsync(ulong companyId, JobStatus? status = null)
    {
        var query = _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.City)
            .Include(j => j.Industry)
            .Include(j => j.Skills)
                .ThenInclude(js => js.Skill)
            .Where(j => j.CompanyId == companyId);

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        return await query
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<Job?> GetJobByIdForEmployerAsync(ulong jobId, ulong companyId)
    {
        return await _context.Jobs
            .Include(j => j.Company)
            .Include(j => j.City)
            .Include(j => j.Industry)
            .Include(j => j.Skills)
                .ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == jobId && j.CompanyId == companyId);
    }

    public async Task<bool> IsJobOwnedByCompanyAsync(ulong jobId, ulong companyId)
    {
        return await _context.Jobs
            .AnyAsync(j => j.Id == jobId && j.CompanyId == companyId);
    }

    public async Task<bool> CloseJobAsync(ulong jobId)
    {
        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null)
            return false;

        job.Status = JobStatus.Closed;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteJobAsync(ulong jobId)
    {
        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null)
            return false;

        // Soft delete by changing status
        job.Status = JobStatus.Closed;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetApplicationCountAsync(ulong jobId)
    {
        return await _context.Applications
            .CountAsync(a => a.JobId == jobId);
    }

    public async Task AddJobSkillsAsync(ulong jobId, List<uint> skillIds)
    {
        var jobSkills = skillIds.Select(skillId => new JobSkill
        {
            JobId = jobId,
            SkillId = skillId
        }).ToList();

        _context.JobSkills.AddRange(jobSkills);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveJobSkillsAsync(ulong jobId)
    {
        var jobSkills = await _context.JobSkills
            .Where(js => js.JobId == jobId)
            .ToListAsync();

        _context.JobSkills.RemoveRange(jobSkills);
        await _context.SaveChangesAsync();
    }
}
