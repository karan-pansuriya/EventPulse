using EventPulse.DAL.Enums;

namespace EventPulse.BLL.DTOs.Booking;

public class PagedBookingProjection
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public int EventId { get; set; }
    public string EventTitle { get; set; } = "";
    public string? VenueName { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string UniqueCode { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
