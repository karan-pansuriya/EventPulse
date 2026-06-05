using EventPulse.BLL.Hubs;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EventPulse.BLL.Services;

public class SignalRSeatUpdateNotifier : ISeatUpdateNotifier
{
    private readonly IHubContext<SeatHub> _hubContext;

    public SignalRSeatUpdateNotifier(IHubContext<SeatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifySeatUpdated(int eventId, int remainingSeats)
    {
        await _hubContext.Clients
            .Group($"event-{eventId}")
            .SendAsync("SeatUpdated", new { eventId, remainingSeats });
    }
}
