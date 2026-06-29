namespace EventPulse.BLL.DTOs.Booking;

public class AdminBookingResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string? VenueName { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string UniqueCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
