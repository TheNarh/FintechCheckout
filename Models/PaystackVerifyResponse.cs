using System.Text.Json.Serialization;

namespace FintechCheckout.Models;

public class PaystackVerifyResponse
{
    public bool Status { get; set; }

    public string Message { get; set; } = string.Empty;

    public PaystackVerifyData Data { get; set; } = new();
}

public class PaystackVerifyData
{
    public string Status { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("gateway_response")]
    public string GatewayResponse { get; set; } = string.Empty;

    [JsonPropertyName("paid_at")]
    public DateTime? PaidAt { get; set; }
}