using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Data.Seeders;

public class SkillsSeeder
{
    private readonly HirenixDbContext _context;

    public SkillsSeeder(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if skills already exist
        if (await _context.Skills.AnyAsync())
        {
            Console.WriteLine("⏭️  Skills already seeded. Skipping...");
            return;
        }

        Console.WriteLine("📚 Seeding skills...");

        // Get skills data
        var skillsData = SkillsSeedData.GetSkills();
        
        // Map to entities
        var skills = skillsData.Select(s => new Skill
        {
            Name = s.Name,
            Slug = s.Slug,
            Category = s.Category
        }).ToList();

        // Bulk insert
        await _context.Skills.AddRangeAsync(skills);
        await _context.SaveChangesAsync();
        
        // Show summary by category
        var categorySummary = skills
            .GroupBy(s => s.Category)
            .Select(g => $"   • {g.Key}: {g.Count()} skills")
            .ToList();
        
        Console.WriteLine($"✅ Seeded {skills.Count} skills across {skills.Select(s => s.Category).Distinct().Count()} categories:");
        foreach (var summary in categorySummary)
        {
            Console.WriteLine(summary);
        }
    }
}
