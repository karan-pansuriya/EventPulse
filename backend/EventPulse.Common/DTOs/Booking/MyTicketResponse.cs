namespace EventPulse.BLL.DTOs.Booking;

public class MyTicketResponse
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string EventTitle { get; set; } = string.Empty;
    public DateTime ? EventDate { get; set; }
    public string? VenueName { get; set; }
    public string? VenueCity { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TicketDto> Tickets { get; set; } = [];
}