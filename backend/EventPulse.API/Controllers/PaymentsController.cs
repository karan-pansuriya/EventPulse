using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.DTOs.Payment;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Authorize(Policy = "CustomerOnly")]
[Route("api/payments")]
public class PaymentsController : BaseHelper
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("create-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        int userId = GetUserId();
        PaymentIntentResponse result = await _paymentService.CreatePaymentIntentAsync(request, userId);
        return SuccessResponse(result);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        BookingResponse result = await _paymentService.ConfirmPaymentAsync(request.PaymentIntentId);
        return SuccessResponse(result);
    }

    [HttpGet("booking/{paymentIntentId}")]
    public async Task<IActionResult> GetBookingByPayment(string paymentIntentId)
    {
        BookingResponse? result = await _paymentService.GetByPaymentIntentAsync(paymentIntentId);
        if (result == null)
            return NotFoundResponse("Booking not found.");
        return SuccessResponse(result);
    }

    [HttpGet("status/{paymentIntentId}")]
    public async Task<IActionResult> GetPaymentStatus(string paymentIntentId)
    {
        var result = await _paymentService.GetPaymentStatusAsync(paymentIntentId);
        return SuccessResponse(result);
    }
}
