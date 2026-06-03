using Hirenix.Domain.Entities;

namespace Hirenix.Application.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(ulong id);
    Task<Conversation?> GetByIdForUserAsync(ulong conversationId, ulong userId);
    Task<Conversation?> GetByParticipantsAsync(ulong user1Id, ulong user2Id);
    Task<List<Conversation>> GetForUserAsync(ulong userId);
    Task<Conversation> CreateAsync(Conversation conversation);
    Task UpdateAsync(Conversation conversation);

    Task<List<Message>> GetMessagesAsync(ulong conversationId, int page = 1, int pageSize = 30);
    Task<Message> CreateMessageAsync(Message message);
    Task<int> MarkMessagesAsReadAsync(ulong conversationId, ulong readerUserId);
    Task<int> GetUnreadCountAsync(ulong conversationId, ulong userId);
    Task<Message?> GetLatestMessageAsync(ulong conversationId);
}
