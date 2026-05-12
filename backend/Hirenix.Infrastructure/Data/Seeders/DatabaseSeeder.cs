namespace Hirenix.Infrastructure.Data.Seeders;

public class DatabaseSeeder
{
    private readonly HirenixDbContext _context;

    public DatabaseSeeder(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task SeedAllAsync()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("🌱 Starting Hirenix Database Seeding...");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");
        
        var startTime = DateTime.Now;
        
        try
        {
            // Seed Skills
            var skillsSeeder = new SkillsSeeder(_context);
            await skillsSeeder.SeedAsync();
            Console.WriteLine();
            
            // Seed Industries
            var industriesSeeder = new IndustriesSeeder(_context);
            await industriesSeeder.SeedAsync();
            Console.WriteLine();
            
            // Seed Locations
            var locationsSeeder = new LocationsSeeder(_context);
            await locationsSeeder.SeedAsync();
            Console.WriteLine();
            
            var duration = DateTime.Now - startTime;
            
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($"✅ Database seeding completed successfully!");
            Console.WriteLine($"⏱️  Total time: {duration.TotalSeconds:F2} seconds");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n═══════════════════════════════════════════════════════════");
            Console.WriteLine("❌ Database seeding failed!");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            throw;
        }
    }
}
