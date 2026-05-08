using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HirenixDbContext _context;

    public UserRepository(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(ulong id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Phone == phone);
    }

    public async Task<User?> GetByProviderIdAsync(string provider, string providerId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u =>
                u.AuthProviderId == providerId &&
                u.AuthProvider.ToString().ToLower() == provider.ToLower());
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> PhoneExistsAsync(string phone)
    {
        return await _context.Users.AnyAsync(u => u.Phone == phone);
    }
}
