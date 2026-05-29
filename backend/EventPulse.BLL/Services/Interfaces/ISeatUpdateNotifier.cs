namespace EventPulse.BLL.Interfaces;

public interface ISeatUpdateNotifier
{
    Task NotifySeatUpdated(int eventId, int remainingSeats);
}
