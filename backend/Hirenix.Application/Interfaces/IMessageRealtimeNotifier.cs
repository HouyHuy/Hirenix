using Hirenix.Application.DTOs.Message;

namespace Hirenix.Application.Interfaces;

public interface IMessageRealtimeNotifier
{
    Task NotifyMessageReceivedAsync(ulong recipientUserId, MessageDto message);
}
