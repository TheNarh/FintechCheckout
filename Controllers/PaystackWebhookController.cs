using FintechCheckout.Configuration;
using FintechCheckout.Data;
using FintechCheckout.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace FintechCheckout.Controllers;

[ApiController]
[Route("api/webhooks/paystack")]
public class PaystackWebhookController : ControllerBase
{
    private readonly PaystackSettings _settings;
    private readonly AppDbContext _db;

    public PaystackWebhookController(
    IOptions<PaystackSettings> settings,
    AppDbContext db)
{
    _settings = settings.Value;
    _db = db;
}

    [HttpPost]
public async Task<IActionResult> HandleWebhook()
{
    var signature = Request.Headers["x-paystack-signature"].ToString();

    if (string.IsNullOrEmpty(signature))
    {
        return Unauthorized(new
        {
            message = "Missing Paystack signature."
        });
    }

    using var reader = new StreamReader(Request.Body);

    var requestBody = await reader.ReadToEndAsync();

    using var hmac = new HMACSHA512(
        Encoding.UTF8.GetBytes(_settings.SecretKey));

    var hash = hmac.ComputeHash(
        Encoding.UTF8.GetBytes(requestBody));

    byte[] providedSignature;

try
{
    providedSignature = Convert.FromHexString(signature);
}
catch (FormatException)
{
    return Unauthorized(new
    {
        message = "Invalid Paystack signature."
    });
}

if (!CryptographicOperations.FixedTimeEquals(
        hash,
        providedSignature))
{
    return Unauthorized(new
    {
        message = "Invalid Paystack signature."
    });
}

    var webhookRequest =
        JsonSerializer.Deserialize<PaystackWebhookRequest>(
            requestBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

    if (webhookRequest == null)
    {
        return BadRequest(new
        {
            message = "Invalid webhook payload."
        });
    }

    if (!string.Equals(
        webhookRequest.Event,
        "charge.success",
        StringComparison.OrdinalIgnoreCase))
{
    return Ok(new
    {
        message = "Webhook event ignored.",
        eventType = webhookRequest.Event
    });
}

   var transaction = await _db.Transactions
    .FirstOrDefaultAsync(
        t => t.Reference == webhookRequest.Data.Reference);

if (transaction == null)
{
    return NotFound(new
    {
        message = "Transaction not found."
    });
}

var expectedAmount = (int)(transaction.Amount * 100);

if (webhookRequest.Data.Amount != expectedAmount)
{
    return BadRequest(new
    {
        message = "Webhook amount does not match the transaction amount."
    });
}

if (!string.Equals(
        webhookRequest.Data.Currency,
        transaction.Currency,
        StringComparison.OrdinalIgnoreCase))
{
    return BadRequest(new
    {
        message = "Webhook currency does not match the transaction currency."
    });
}

if (transaction.Status == TransactionStatus.Success)
{
    return Ok(new
    {
        message = "Transaction already processed.",
        transaction.Reference,
        transaction.Status
    });
}

var paystackStatus = webhookRequest.Data.Status.ToLowerInvariant();

if (paystackStatus == "success")
{
    transaction.Status = TransactionStatus.Success;
    transaction.PaidAt = webhookRequest.Data.PaidAt;
    transaction.GatewayResponse = webhookRequest.Data.GatewayResponse;
    transaction.UpdatedAt = DateTime.UtcNow;
}
else if (paystackStatus == "failed")
{
    transaction.Status = TransactionStatus.Failed;
    transaction.GatewayResponse = webhookRequest.Data.GatewayResponse;
    transaction.UpdatedAt = DateTime.UtcNow;
}
else
{
    return BadRequest(new
    {
        message = $"Unsupported Paystack transaction status: {paystackStatus}"
    });
}

await _db.SaveChangesAsync();

return Ok(new
{
    message = "Transaction already processed.",
    transaction.Reference,
    Status = transaction.Status.ToString()
});
}
}