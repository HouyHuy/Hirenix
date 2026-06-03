using Hirenix.Application.Interfaces;
using Hirenix.Domain.Entities;
using Hirenix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hirenix.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly HirenixDbContext _context;

    public ConversationRepository(HirenixDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(ulong id)
    {
        return await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Conversation?> GetByIdForUserAsync(ulong conversationId, ulong userId)
    {
        return await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .FirstOrDefaultAsync(c =>
                c.Id == conversationId &&
                (c.User1Id == userId || c.User2Id == userId));
    }

    public async Task<Conversation?> GetByParticipantsAsync(ulong user1Id, ulong user2Id)
    {
        var min = Math.Min(user1Id, user2Id);
        var max = Math.Max(user1Id, user2Id);

        return await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .FirstOrDefaultAsync(c => c.User1Id == min && c.User2Id == max);
    }

    public async Task<List<Conversation>> GetForUserAsync(ulong userId)
    {
        return await _context.Conversations
            .AsNoTracking()
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Conversation> CreateAsync(Conversation conversation)
    {
        await _context.Conversations.AddAsync(conversation);
        await _context.SaveChangesAsync();
        return conversation;
    }

    public async Task UpdateAsync(Conversation conversation)
    {
        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Message>> GetMessagesAsync(ulong conversationId, int page = 1, int pageSize = 30)
    {
        var safePage = Math.Max(page, 1);
        var safeSize = Math.Clamp(pageSize, 1, 100);
        var skip = (safePage - 1) * safeSize;

        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(safeSize)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Message> CreateMessageAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<int> MarkMessagesAsReadAsync(ulong conversationId, ulong readerUserId)
    {
        var updated = await _context.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderId != readerUserId && !m.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.IsRead, true));

        return updated;
    }

    public async Task<int> GetUnreadCountAsync(ulong conversationId, ulong userId)
    {
        return await _context.Messages
            .AsNoTracking()
            .CountAsync(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead);
    }

    public async Task<Message?> GetLatestMessageAsync(ulong conversationId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
