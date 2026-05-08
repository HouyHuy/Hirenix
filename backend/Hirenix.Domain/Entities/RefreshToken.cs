namespace Hirenix.Domain.Entities;

public class RefreshToken
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;

    // Navigation property
    public User User { get; set; } = null!;
}
