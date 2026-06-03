using Hirenix.Application.DTOs.Message;

namespace Hirenix.Application.Interfaces;

public interface IMessageService
{
    Task<List<ConversationSummaryDto>> GetConversationsAsync(ulong userId);
    Task<ConversationSummaryDto?> GetConversationAsync(ulong userId, ulong conversationId);
    Task<List<MessageDto>> GetMessagesAsync(ulong userId, ulong conversationId, int page = 1, int pageSize = 30);
    Task<ConversationSummaryDto> CreateConversationAsync(ulong userId, CreateConversationDto dto);
    Task<MessageDto> SendMessageAsync(ulong userId, ulong conversationId, SendMessageDto dto);
    Task<int> MarkAsReadAsync(ulong userId, ulong conversationId);
}
