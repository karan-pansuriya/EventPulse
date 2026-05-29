using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[ApiController]
[Route("api/webhooks")]
public class StripeWebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public StripeWebhookController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("stripe")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        string signature = Request.Headers["Stripe-Signature"]!;

        try
        {
            await _paymentService.HandleWebhookAsync(json, signature);
            return Ok();
        }
        catch (Stripe.StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
