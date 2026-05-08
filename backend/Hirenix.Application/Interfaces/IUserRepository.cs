using Hirenix.Domain.Entities;

namespace Hirenix.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(ulong id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string phone);
    Task<User?> GetByProviderIdAsync(string provider, string providerId);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);
}
