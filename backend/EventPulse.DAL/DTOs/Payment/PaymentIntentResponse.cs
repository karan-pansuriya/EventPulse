namespace EventPulse.BLL.DTOs.Payment;

public class PaymentIntentResponse
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public int BookingId { get; set; }
}
