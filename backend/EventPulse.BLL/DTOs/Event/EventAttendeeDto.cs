namespace EventPulse.BLL.DTOs.Event;

public class EventAttendeeDto
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
}
