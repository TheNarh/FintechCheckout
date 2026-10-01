using System.Text.Json.Serialization;

namespace FintechCheckout.Models;

public class PaystackInitializeResponse
{
    public bool Status { get; set; }

    public string Message { get; set; } = string.Empty;

    public PaystackInitializeData Data { get; set; } = new();
}

public class PaystackInitializeData
{
    [JsonPropertyName("authorization_url")]
    public string AuthorizationUrl { get; set; } = string.Empty;

    [JsonPropertyName("access_code")]
    public string AccessCode { get; set; } = string.Empty;

    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;
}