namespace EventPulse.BLL.DTOs.Payment;

public class CreatePaymentIntentRequest
{
    public int EventId { get; set; }
    public int Quantity { get; set; } = 1;
}
