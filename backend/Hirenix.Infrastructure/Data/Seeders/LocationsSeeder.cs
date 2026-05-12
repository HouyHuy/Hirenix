using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Data.Seeders;

public class LocationsSeeder
{
    private readonly HirenixDbContext _context;

    public LocationsSeeder(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if locations already exist
        if (await _context.Locations.AnyAsync())
        {
            Console.WriteLine("⏭️  Locations already seeded. Skipping...");
            return;
        }

        Console.WriteLine("📍 Seeding locations...");

        // Get locations data
        var locationsData = LocationsSeedData.GetLocations();
        
        // Map to entities
        var locations = locationsData.Select(l => new Location
        {
            Name = l.Name,
            Slug = l.Slug,
            CountryCode = "VN"
        }).ToList();

        // Bulk insert
        await _context.Locations.AddRangeAsync(locations);
        await _context.SaveChangesAsync();
        
        Console.WriteLine($"✅ Seeded {locations.Count} Vietnam provinces/cities");
    }
}
