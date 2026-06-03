namespace Hirenix.Application.DTOs.Message;

public class MessageDto
{
    public ulong Id { get; set; }
    public ulong ConversationId { get; set; }
    public ulong SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsMine { get; set; }
}
