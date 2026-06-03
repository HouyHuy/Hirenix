using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Company entity
/// </summary>
public class CompanyRepository : ICompanyRepository
{
    private readonly HirenixDbContext _context;

    public CompanyRepository(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(ulong id)
    {
        return await _context.Companies
            .Include(c => c.Industry)
            .Include(c => c.City)
            .Include(c => c.EmployerProfiles)
            .Include(c => c.Jobs)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
    }

    public async Task<Company?> GetByNameAsync(string name)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && c.IsActive);
    }

    public async Task<(List<Company> Companies, int Total)> GetAllAsync(int page, int pageSize)
    {
        var query = _context.Companies
            .Include(c => c.Industry)
            .Include(c => c.City)
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync();
        var companies = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (companies, total);
    }

    public async Task<Company> CreateAsync(Company company)
    {
        company.CreatedAt = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;
        company.IsActive = true;

        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        return company;
    }

    public async Task<Company> UpdateAsync(Company company)
    {
        company.UpdatedAt = DateTime.UtcNow;

        _context.Companies.Update(company);
        await _context.SaveChangesAsync();

        return company;
    }

    public async Task<bool> DeleteAsync(ulong id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
            return false;

        // Soft delete
        company.IsActive = false;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string name, ulong? excludeId = null)
    {
        var query = _context.Companies
            .Where(c => c.Name.ToLower() == name.ToLower() && c.IsActive);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<Company>> GetByIndustryAsync(uint industryId)
    {
        return await _context.Companies
            .Include(c => c.Industry)
            .Include(c => c.City)
            .Where(c => c.IndustryId == industryId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Company>> GetByCityAsync(uint cityId)
    {
        return await _context.Companies
            .Include(c => c.Industry)
            .Include(c => c.City)
            .Where(c => c.CityId == cityId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
