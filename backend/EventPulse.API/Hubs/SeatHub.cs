using Microsoft.AspNetCore.SignalR;

namespace EventPulse.API.Hubs;

public class SeatHub : Hub
{
    public async Task JoinEventGroup(int eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"event-{eventId}");
    }

    public async Task LeaveEventGroup(int eventId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event-{eventId}");
    }
}
