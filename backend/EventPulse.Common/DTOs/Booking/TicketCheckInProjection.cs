namespace EventPulse.BLL.DTOs.Booking;

public class TicketCheckInProjection
{
    public int TicketId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public int BookingId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int EventOrganizerId { get; set; }
    public string? AttendeeName { get; set; }
}
