namespace Hirenix.Domain.Entities;

public class Conversation
{
    public ulong Id { get; set; }
    public ulong User1Id { get; set; }
    public ulong User2Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User1 { get; set; }
    public User? User2 { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class Message
{
    public ulong Id { get; set; }
    public ulong ConversationId { get; set; }
    public ulong SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Conversation? Conversation { get; set; }
    public User? Sender { get; set; }
}
