using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.DTOs.Payment;

namespace EventPulse.BLL.Interfaces;

public interface IPaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, int userId);
    Task<BookingResponse> ConfirmPaymentAsync(string paymentIntentId);
    Task HandleWebhookAsync(string json, string signatureHeader);
    Task<BookingResponse?> GetByPaymentIntentAsync(string paymentIntentId);
    Task<object> GetPaymentStatusAsync(string paymentIntentId);
}
