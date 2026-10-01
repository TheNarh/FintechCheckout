using System.Text.Json.Serialization;

namespace FintechCheckout.Models;

public class PaystackWebhookRequest
{
    public string Event { get; set; } = string.Empty;

    public PaystackWebhookData Data { get; set; } = new();
}

public class PaystackWebhookData
{
    public string Status { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public int Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("gateway_response")]
    public string GatewayResponse { get; set; } = string.Empty;

    [JsonPropertyName("paid_at")]
    public DateTime? PaidAt { get; set; }
}