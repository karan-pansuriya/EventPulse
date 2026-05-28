using AutoMapper;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.DTOs.Payment;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.BLL.Models.Configuration;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Stripe;
using StripeEvent = Stripe.Event;

namespace EventPulse.BLL.Services;

public class PaymentService : IPaymentService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly StripeSettings _stripeSettings;
    private readonly ISeatUpdateNotifier _seatNotifier;

    public PaymentService(
        IBookingRepository bookingRepository,
        IMapper mapper,
        IOptions<StripeSettings> stripeSettings,
        ISeatUpdateNotifier seatNotifier)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _stripeSettings = stripeSettings.Value;
        _seatNotifier = seatNotifier;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, int userId)
    {
        if (request.Quantity < 1)
            throw new BadRequestException("Quantity must be at least 1.");

        var eventEntity = await _bookingRepository.GetEventByIdAsync(request.EventId)
            ?? throw new NotFoundException("Event not found.");

        if (request.Quantity > eventEntity.TotalSeats)
            throw new BadRequestException($"Only {eventEntity.TotalSeats} seats available.");

        decimal totalAmount = eventEntity.Price * request.Quantity;
        long amountInCents = (long)(totalAmount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = "usd",
            Metadata = new Dictionary<string, string>
            {
                { "event_id", request.EventId.ToString() },
                { "user_id", userId.ToString() },
                { "quantity", request.Quantity.ToString() },
            },
        };

        var service = new PaymentIntentService();
        PaymentIntent paymentIntent = await service.CreateAsync(options);

        string uniqueCode = GenerateUniqueCode();

        Booking booking = await _bookingRepository.CreatePendingBookingAsync(
            userId, request.EventId, uniqueCode, request.Quantity,
            eventEntity.Price, totalAmount, paymentIntent.Id);

        return new PaymentIntentResponse
        {
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id,
            BookingId = booking.Id,
        };
    }

    public async Task<BookingResponse> ConfirmPaymentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        PaymentIntent paymentIntent = await service.GetAsync(paymentIntentId);

        if (paymentIntent.Status != "succeeded")
            throw new BadRequestException($"Payment not successful. Status: {paymentIntent.Status}");

        (Booking booking, int remainingSeats) = await _bookingRepository.ConfirmPaymentAsync(paymentIntentId);

        await _seatNotifier.NotifySeatUpdated(booking.EventId, remainingSeats);

        Booking? bookingWithDetails = await _bookingRepository.GetBookingWithDetailsAsync(booking.Id);

        BookingResponse response = _mapper.Map<BookingResponse>(bookingWithDetails);
        response.RemainingSeats = remainingSeats;
        return response;
    }

    public async Task HandleWebhookAsync(string json, string signatureHeader)
    {
        try
        {
            StripeEvent stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _stripeSettings.WebhookSecret);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        (Booking booking, int remainingSeats) = await _bookingRepository.ConfirmPaymentAsync(paymentIntent.Id);
                        await _seatNotifier.NotifySeatUpdated(booking.EventId, remainingSeats);
                    }
                    break;

                case "payment_intent.payment_failed":
                    var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (failedIntent != null)
                    {
                        await _bookingRepository.MarkPaymentFailedAsync(failedIntent.Id);
                    }
                    break;
            }
        }
        catch (StripeException)
        {
            throw;
        }
    }

    public async Task<BookingResponse?> GetByPaymentIntentAsync(string paymentIntentId)
    {
        Booking? booking = await _bookingRepository.GetByPaymentIntentAsync(paymentIntentId);
        return booking == null ? null : _mapper.Map<BookingResponse>(booking);
    }

    public async Task<object> GetPaymentStatusAsync(string paymentIntentId)
    {
        Booking? booking = await _bookingRepository.GetByPaymentIntentAsync(paymentIntentId);
        if (booking == null)
            return new { status = "not_found" };

        return new
        {
            status = booking.PaymentStatus.ToString().ToLower(),
            bookingId = booking.Id,
            uniqueCode = booking.UniqueCode,
        };
    }

    private static string GenerateUniqueCode()
    {
        return $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}
