using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Booking;

public class CheckInRequest
{
    [MinLength(10, ErrorMessage = "Ticket code must be at least 10 characters.")]
    [MaxLength(50, ErrorMessage = "Ticket code must be at most 50 characters.")]
    [Required(ErrorMessage = "Ticket code is required.")]
    public string TicketCode { get; set; } = string.Empty;
}
