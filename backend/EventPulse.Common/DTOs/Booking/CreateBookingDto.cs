namespace EventPulse.BLL.DTOs.Booking;

public class CreateBookingDto
{
    public int EventId { get; set; }
    public int Quantity { get; set; } = 1;
}
