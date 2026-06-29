namespace EventPulse.BLL.DTOs.Booking;

public class BookingResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string UniqueCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PricePerTicket { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentRef { get; set; }
    public List<TicketDto> Tickets { get; set; } = [];
    public List<string> TicketCodes { get; set; } = [];
    public int RemainingSeats { get; set; }
    public DateTime CreatedAt { get; set; }
}
