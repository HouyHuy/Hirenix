using Hirenix.Domain.Enums;

namespace Hirenix.Application.DTOs.Auth;

public class RegisterRequestDto
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Candidate; // Default to Candidate
}
