using Hirenix.Application.DTOs.Message;
using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;

namespace Hirenix.Application.Services;

public class MessageService : IMessageService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICandidateProfileRepository _candidateProfileRepository;
    private readonly IEmployerProfileRepository _employerProfileRepository;
    private readonly IMessageRealtimeNotifier _messageRealtimeNotifier;

    public MessageService(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        ICandidateProfileRepository candidateProfileRepository,
        IEmployerProfileRepository employerProfileRepository,
        IMessageRealtimeNotifier messageRealtimeNotifier)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _candidateProfileRepository = candidateProfileRepository;
        _employerProfileRepository = employerProfileRepository;
        _messageRealtimeNotifier = messageRealtimeNotifier;
    }

    public async Task<List<ConversationSummaryDto>> GetConversationsAsync(ulong userId)
    {
        var conversations = await _conversationRepository.GetForUserAsync(userId);
        var userIds = conversations.SelectMany(c => new[] { c.User1Id, c.User2Id }).Distinct();
        var users = await _userRepository.GetByIdsAsync(userIds);
        var userMap = users.ToDictionary(u => u.Id, u => u);

        var result = new List<ConversationSummaryDto>(conversations.Count);

        foreach (var conversation in conversations)
        {
            var participantId = conversation.User1Id == userId ? conversation.User2Id : conversation.User1Id;
            var participantUser = userMap.GetValueOrDefault(participantId);
            var latest = await _conversationRepository.GetLatestMessageAsync(conversation.Id);
            var unread = await _conversationRepository.GetUnreadCountAsync(conversation.Id, userId);

            var participantName = await ResolveDisplayNameAsync(participantId, participantUser);
            var avatarUrl = await ResolveAvatarAsync(participantId);

            result.Add(new ConversationSummaryDto
            {
                Id = conversation.Id,
                ParticipantUserId = participantId,
                ParticipantName = participantName,
                ParticipantRole = participantUser?.Role.ToString() ?? string.Empty,
                ParticipantAvatarUrl = avatarUrl,
                LastMessage = latest?.Content,
                LastMessageAt = latest?.CreatedAt,
                UnreadCount = unread,
                UpdatedAt = conversation.UpdatedAt
            });
        }

        return result
            .OrderByDescending(c => c.LastMessageAt ?? c.UpdatedAt)
            .ToList();
    }

    public async Task<ConversationSummaryDto?> GetConversationAsync(ulong userId, ulong conversationId)
    {
        var conversation = await _conversationRepository.GetByIdForUserAsync(conversationId, userId);
        if (conversation == null)
        {
            return null;
        }

        var participantId = conversation.User1Id == userId ? conversation.User2Id : conversation.User1Id;
        var participantUser = await _userRepository.GetByIdAsync(participantId);
        var latest = await _conversationRepository.GetLatestMessageAsync(conversation.Id);
        var unread = await _conversationRepository.GetUnreadCountAsync(conversation.Id, userId);

        return new ConversationSummaryDto
        {
            Id = conversation.Id,
            ParticipantUserId = participantId,
            ParticipantName = await ResolveDisplayNameAsync(participantId, participantUser),
            ParticipantRole = participantUser?.Role.ToString() ?? string.Empty,
            ParticipantAvatarUrl = await ResolveAvatarAsync(participantId),
            LastMessage = latest?.Content,
            LastMessageAt = latest?.CreatedAt,
            UnreadCount = unread,
            UpdatedAt = conversation.UpdatedAt
        };
    }

    public async Task<List<MessageDto>> GetMessagesAsync(ulong userId, ulong conversationId, int page = 1, int pageSize = 30)
    {
        var conversation = await _conversationRepository.GetByIdForUserAsync(conversationId, userId);
        if (conversation == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this conversation");
        }

        var messages = await _conversationRepository.GetMessagesAsync(conversationId, page, pageSize);

        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.Email ?? "Unknown",
            SenderRole = m.Sender?.Role.ToString() ?? string.Empty,
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt,
            IsMine = m.SenderId == userId
        }).ToList();
    }

    public async Task<ConversationSummaryDto> CreateConversationAsync(ulong userId, CreateConversationDto dto)
    {
        if (dto.ParticipantUserId == userId)
        {
            throw new ArgumentException("Cannot create conversation with yourself");
        }

        var currentUser = await _userRepository.GetByIdAsync(userId);
        var participant = await _userRepository.GetByIdAsync(dto.ParticipantUserId);

        if (currentUser == null || participant == null)
        {
            throw new ArgumentException("User not found");
        }

        var existing = await _conversationRepository.GetByParticipantsAsync(userId, dto.ParticipantUserId);
        if (existing != null)
        {
            var existingSummary = await GetConversationAsync(userId, existing.Id);
            return existingSummary!;
        }

        var min = Math.Min(userId, dto.ParticipantUserId);
        var max = Math.Max(userId, dto.ParticipantUserId);

        var conversation = await _conversationRepository.CreateAsync(new Conversation
        {
            User1Id = min,
            User2Id = max,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return (await GetConversationAsync(userId, conversation.Id))!;
    }

    public async Task<MessageDto> SendMessageAsync(ulong userId, ulong conversationId, SendMessageDto dto)
    {
        var conversation = await _conversationRepository.GetByIdForUserAsync(conversationId, userId);
        if (conversation == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this conversation");
        }

        var content = dto.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty");
        }

        var message = await _conversationRepository.CreateMessageAsync(new Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        conversation.UpdatedAt = DateTime.UtcNow;
        await _conversationRepository.UpdateAsync(conversation);

        var sender = await _userRepository.GetByIdAsync(userId);

        var messageDto = new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = await ResolveDisplayNameAsync(userId, sender),
            SenderRole = sender?.Role.ToString() ?? string.Empty,
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt,
            IsMine = true
        };

        var recipientUserId = conversation.User1Id == userId ? conversation.User2Id : conversation.User1Id;
        await _messageRealtimeNotifier.NotifyMessageReceivedAsync(recipientUserId, new MessageDto
        {
            Id = messageDto.Id,
            ConversationId = messageDto.ConversationId,
            SenderId = messageDto.SenderId,
            SenderName = messageDto.SenderName,
            SenderRole = messageDto.SenderRole,
            Content = messageDto.Content,
            IsRead = messageDto.IsRead,
            CreatedAt = messageDto.CreatedAt,
            IsMine = false
        });

        return messageDto;
    }

    public async Task<int> MarkAsReadAsync(ulong userId, ulong conversationId)
    {
        var conversation = await _conversationRepository.GetByIdForUserAsync(conversationId, userId);
        if (conversation == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this conversation");
        }

        return await _conversationRepository.MarkMessagesAsReadAsync(conversationId, userId);
    }

    private async Task<string> ResolveDisplayNameAsync(ulong userId, User? user)
    {
        if (user == null)
        {
            return "Unknown";
        }

        var employerProfile = await _employerProfileRepository.GetByUserIdAsync(userId);
        if (!string.IsNullOrWhiteSpace(employerProfile?.FullName))
        {
            return employerProfile.FullName;
        }

        var candidateProfile = await _candidateProfileRepository.GetByUserIdAsync(userId);
        if (!string.IsNullOrWhiteSpace(candidateProfile?.FullName))
        {
            return candidateProfile.FullName;
        }

        return user.Email ?? user.Phone ?? $"User {userId}";
    }

    private async Task<string?> ResolveAvatarAsync(ulong userId)
    {
        var candidateProfile = await _candidateProfileRepository.GetByUserIdAsync(userId);
        return candidateProfile?.AvatarUrl;
    }
}
