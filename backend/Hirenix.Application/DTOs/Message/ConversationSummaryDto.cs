namespace Hirenix.Application.DTOs.Message;

public class ConversationSummaryDto
{
    public ulong Id { get; set; }
    public ulong ParticipantUserId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantRole { get; set; } = string.Empty;
    public string? ParticipantAvatarUrl { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
