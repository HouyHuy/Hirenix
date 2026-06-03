using Hirenix.Application.Interfaces;
using Hirenix.Domain.Enums;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AppEntity = Hirenix.Domain.Entities.Application;

namespace Hirenix.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly HirenixDbContext _context;

    public ApplicationRepository(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task<AppEntity> CreateApplicationAsync(AppEntity application)
    {
        await _context.Applications.AddAsync(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<AppEntity?> GetApplicationByIdAsync(ulong id)
    {
        return await _context.Applications
            .Include(a => a.Job)
                .ThenInclude(j => j.Company)
            .Include(a => a.Job)
                .ThenInclude(j => j.City)
            .Include(a => a.Candidate)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AppEntity>> GetCandidateApplicationsAsync(ulong candidateId)
    {
        return await _context.Applications
            .Include(a => a.Job)
                .ThenInclude(j => j.Company)
            .Include(a => a.Job)
                .ThenInclude(j => j.City)
            .Where(a => a.CandidateId == candidateId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<bool> HasAppliedAsync(ulong jobId, ulong candidateId)
    {
        return await _context.Applications
            .AnyAsync(a => a.JobId == jobId && a.CandidateId == candidateId);
    }

    public async Task<AppEntity?> GetApplicationByJobAndCandidateAsync(ulong jobId, ulong candidateId)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.CandidateId == candidateId);
    }

    public async Task UpdateApplicationAsync(AppEntity application)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteApplicationAsync(AppEntity application)
    {
        _context.Applications.Remove(application);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetJobApplicationsCountAsync(ulong jobId)
    {
        return await _context.Applications
            .CountAsync(a => a.JobId == jobId);
    }

    public async Task<List<AppEntity>> GetApplicationsForEmployerAsync(
        ulong employerUserId,
        ulong? jobId = null,
        ApplicationStatus? status = null)
    {
        var query = _context.Applications
            .AsNoTracking()
            .Include(a => a.Job)
                .ThenInclude(j => j.Company)
            .Include(a => a.Candidate)
            .Include(a => a.Reviewer)
            .Include(a => a.Job)
                .ThenInclude(j => j.City)
            .Include(a => a.Job)
                .ThenInclude(j => j.Industry)
            .Where(a => _context.EmployerProfiles.Any(ep => ep.UserId == employerUserId && ep.CompanyId == a.Job.CompanyId));

        if (jobId.HasValue)
        {
            query = query.Where(a => a.JobId == jobId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    public async Task<AppEntity?> GetApplicationWithDetailsForEmployerAsync(ulong employerUserId, ulong applicationId)
    {
        return await _context.Applications
            .Include(a => a.Job)
                .ThenInclude(j => j.Company)
            .Include(a => a.Candidate)
            .Include(a => a.Reviewer)
            .Include(a => a.Job)
                .ThenInclude(j => j.City)
            .Include(a => a.Job)
                .ThenInclude(j => j.Industry)
            .FirstOrDefaultAsync(a =>
                a.Id == applicationId &&
                _context.EmployerProfiles.Any(ep => ep.UserId == employerUserId && ep.CompanyId == a.Job.CompanyId));
    }

    public async Task<Dictionary<ApplicationStatus, int>> GetApplicationsStatsByEmployerAsync(ulong employerUserId)
    {
        var grouped = await _context.Applications
            .AsNoTracking()
            .Where(a => _context.EmployerProfiles.Any(ep => ep.UserId == employerUserId && ep.CompanyId == a.Job.CompanyId))
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return grouped.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<Dictionary<ulong, int>> GetApplicationsCountByJobAsync(ulong employerUserId)
    {
        var grouped = await _context.Applications
            .AsNoTracking()
            .Where(a => _context.EmployerProfiles.Any(ep => ep.UserId == employerUserId && ep.CompanyId == a.Job.CompanyId))
            .GroupBy(a => a.JobId)
            .Select(g => new { JobId = g.Key, Count = g.Count() })
            .ToListAsync();

        return grouped.ToDictionary(x => x.JobId, x => x.Count);
    }
}
