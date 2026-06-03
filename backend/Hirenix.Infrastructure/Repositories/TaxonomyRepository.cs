using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Repositories;

public class TaxonomyRepository : ITaxonomyRepository
{
    private readonly HirenixDbContext _context;

    public TaxonomyRepository(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task<List<Location>> GetAllLocationsAsync()
    {
        return await _context.Locations
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<List<Industry>> GetAllIndustriesAsync()
    {
        return await _context.Industries
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<List<Skill>> GetAllSkillsAsync()
    {
        return await _context.Skills
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
}
