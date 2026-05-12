using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Data.Seeders;

public class IndustriesSeeder
{
    private readonly HirenixDbContext _context;

    public IndustriesSeeder(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if industries already exist
        if (await _context.Industries.AnyAsync())
        {
            Console.WriteLine("⏭️  Industries already seeded. Skipping...");
            return;
        }

        Console.WriteLine("🏢 Seeding industries...");

        // Get industries data
        var industriesData = IndustriesSeedData.GetIndustries();
        
        // Map to entities
        var industries = industriesData.Select(i => new Industry
        {
            Name = i.Name,
            Slug = i.Slug
        }).ToList();

        // Bulk insert
        await _context.Industries.AddRangeAsync(industries);
        await _context.SaveChangesAsync();
        
        Console.WriteLine($"✅ Seeded {industries.Count} industries");
    }
}
