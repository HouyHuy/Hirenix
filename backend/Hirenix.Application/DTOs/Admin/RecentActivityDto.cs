namespace Hirenix.Application.DTOs.Admin;

public class RecentActivityDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public ulong? UserId { get; set; }
    public string? UserName { get; set; }
    public ulong? JobId { get; set; }
    public string? JobTitle { get; set; }
    public string? CompanyName { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
