namespace EventPulse.BLL.DTOs.Booking;

public class TicketDto
{
    public int Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string? QrCodeUrl { get; set; }
    public string? PdfUrl { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
}
