using Hirenix.API.Hubs;
using Hirenix.Application.DTOs.Message;
using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Hirenix.API.Realtime;

public class SignalRMessageRealtimeNotifier : IMessageRealtimeNotifier
{
    private readonly IHubContext<MessagesHub> _hubContext;

    public SignalRMessageRealtimeNotifier(IHubContext<MessagesHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyMessageReceivedAsync(ulong recipientUserId, MessageDto message)
    {
        var group = MessagesHub.GetUserGroup(recipientUserId.ToString());
        return _hubContext.Clients.Group(group).SendAsync("MessageReceived", message);
    }
}
