using Hirenix.Domain.Entities;
using Hirenix.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Data.Seeders;

public class UsersSeeder
{
    private readonly HirenixDbContext _context;

    public UsersSeeder(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        Console.WriteLine("📝 Seeding Users...");

        // Check if users already exist
        var existingEmails = await _context.Users
            .Where(u => u.Email != null)
            .Select(u => u.Email!)
            .ToListAsync();
        var existingEmailSet = existingEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var users = new List<User>
        {
            // Admin Account
            new User
            {
                Email = "admin@hirenix.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                AuthProvider = AuthProvider.Email,
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            
            // Candidate Account
            new User
            {
                Email = "candidate@hirenix.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Candidate@123"),
                Role = UserRole.Candidate,
                AuthProvider = AuthProvider.Email,
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            
            // Employer Account
            new User
            {
                Email = "employer@hirenix.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employer@123"),
                Role = UserRole.Employer,
                AuthProvider = AuthProvider.Email,
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var usersToCreate = users
            .Where(u => !string.IsNullOrWhiteSpace(u.Email) && !existingEmailSet.Contains(u.Email))
            .ToList();

        if (usersToCreate.Count == 0)
        {
            Console.WriteLine("   ⏭️  Users already seeded. Skipping...");
            return;
        }

        await _context.Users.AddRangeAsync(usersToCreate);
        await _context.SaveChangesAsync();

        Console.WriteLine($"   ✅ Seeded {usersToCreate.Count} users successfully!");
        Console.WriteLine("      - admin@hirenix.com (Admin@123)");
        Console.WriteLine("      - candidate@hirenix.com (Candidate@123)");
        Console.WriteLine("      - employer@hirenix.com (Employer@123)");
    }
}
