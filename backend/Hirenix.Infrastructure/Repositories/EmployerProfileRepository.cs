using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for EmployerProfile entity
/// </summary>
public class EmployerProfileRepository : IEmployerProfileRepository
{
    private readonly HirenixDbContext _context;

    public EmployerProfileRepository(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task<EmployerProfile?> GetByIdAsync(ulong id)
    {
        return await _context.EmployerProfiles
            .Include(ep => ep.User)
            .Include(ep => ep.Company)
            .FirstOrDefaultAsync(ep => ep.Id == id && ep.IsActive);
    }

    public async Task<EmployerProfile?> GetByUserIdAsync(ulong userId)
    {
        return await _context.EmployerProfiles
            .Include(ep => ep.User)
            .Include(ep => ep.Company)
            .FirstOrDefaultAsync(ep => ep.UserId == userId && ep.IsActive);
    }

    public async Task<List<EmployerProfile>> GetByCompanyIdAsync(ulong companyId)
    {
        return await _context.EmployerProfiles
            .Include(ep => ep.User)
            .Where(ep => ep.CompanyId == companyId && ep.IsActive)
            .OrderBy(ep => ep.FullName)
            .ToListAsync();
    }

    public async Task<EmployerProfile> CreateAsync(EmployerProfile profile)
    {
        profile.CreatedAt = DateTime.UtcNow;
        profile.IsActive = true;

        await _context.EmployerProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    public async Task<EmployerProfile> UpdateAsync(EmployerProfile profile)
    {
        _context.EmployerProfiles.Update(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    public async Task<bool> DeleteAsync(ulong id)
    {
        var profile = await _context.EmployerProfiles.FindAsync(id);
        if (profile == null)
            return false;

        // Soft delete
        profile.IsActive = false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByUserIdAsync(ulong userId)
    {
        return await _context.EmployerProfiles
            .AnyAsync(ep => ep.UserId == userId && ep.IsActive);
    }
}
