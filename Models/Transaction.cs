namespace FintechCheckout.Models;

public class Transaction
{
    public int Id { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "GHS";

    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    public string GatewayResponse { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}