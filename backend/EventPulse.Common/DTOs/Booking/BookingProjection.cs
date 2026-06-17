using EventPulse.DAL.Enums;

namespace EventPulse.BLL.DTOs.Booking;

public class BookingProjection
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
