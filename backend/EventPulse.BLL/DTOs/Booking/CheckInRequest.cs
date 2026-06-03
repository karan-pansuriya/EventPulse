using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Booking;

public class CheckInRequest
{
    [MinLength(10, ErrorMessage = "Ticket code at least 10 characters long.")]
    [Required(ErrorMessage = "Ticket code is required.")]
    public string TicketCode { get; set; } = string.Empty;
}
