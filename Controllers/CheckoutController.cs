using FintechCheckout.Data;
using FintechCheckout.Models;
using FintechCheckout.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FintechCheckout.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PaystackService _paystackService;

    public CheckoutController(
        AppDbContext db,
        PaystackService paystackService)
    {
        _db = db;
        _paystackService = paystackService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCheckout(CheckoutRequest request)
    {
        var transaction = new Transaction
        {
            Reference = $"TXN-{Guid.NewGuid():N}",
            Email = request.Email,
            Amount = request.Amount,
            Currency = "GHS",
            Status = TransactionStatus.Pending
        };

        _db.Transactions.Add(transaction);

        await _db.SaveChangesAsync();

        var paystackResponse =
            await _paystackService.InitializeTransaction(
                transaction.Email,
                transaction.Amount,
                transaction.Reference);

        return Ok(new
        {
            transaction.Id,
            transaction.Reference,
            transaction.Amount,
            transaction.Currency,
            transaction.Status,
            AuthorizationUrl = paystackResponse.Data.AuthorizationUrl,
            AccessCode = paystackResponse.Data.AccessCode,
            PaystackReference = paystackResponse.Data.Reference
        });
    }

    [HttpGet("{reference}/verify")]
public async Task<IActionResult> VerifyPayment(string reference)
{
    var transaction = await _db.Transactions
        .FirstOrDefaultAsync(t => t.Reference == reference);

    if (transaction == null)
    {
        return NotFound(new
        {
            message = "Transaction not found."
        });
    }

    var paystackResponse =
        await _paystackService.VerifyTransaction(reference);

   var paystackStatus = paystackResponse.Data.Status.ToLowerInvariant();

    if (paystackStatus == "success")
{
    transaction.Status = TransactionStatus.Success;
    transaction.PaidAt = paystackResponse.Data.PaidAt;
    transaction.GatewayResponse = paystackResponse.Data.GatewayResponse;
    transaction.UpdatedAt = DateTime.UtcNow;
}
else if (paystackStatus == "failed")
{
    transaction.Status = TransactionStatus.Failed;
    transaction.GatewayResponse = paystackResponse.Data.GatewayResponse;
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
        transaction.Id,
        transaction.Reference,
        transaction.Amount,
        transaction.Currency,
        transaction.Status,
        PaystackStatus = paystackResponse.Data.Status,
        GatewayResponse = paystackResponse.Data.GatewayResponse,
        PaidAt = paystackResponse.Data.PaidAt
    });
}
}