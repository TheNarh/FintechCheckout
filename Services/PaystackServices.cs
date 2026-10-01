using FintechCheckout.Configuration;
using FintechCheckout.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FintechCheckout.Services;

public class PaystackService
{
    private readonly HttpClient _httpClient;
    private readonly PaystackSettings _settings;

    public PaystackService(
        HttpClient httpClient,
        IOptions<PaystackSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
    }

    public async Task<PaystackInitializeResponse> InitializeTransaction(
    string email,
    decimal amount,
    string reference)
{
    var payload = new
    {
        email = email,
        amount = (int)(amount * 100),
        currency = "GHS",
        reference = reference
    };

    var json = JsonSerializer.Serialize(payload);

    var content = new StringContent(
        json,
        Encoding.UTF8,
        "application/json");

    var response = await _httpClient.PostAsync(
        "/transaction/initialize",
        content);

    response.EnsureSuccessStatusCode();

    var responseJson = await response.Content.ReadAsStringAsync();

    var result = JsonSerializer.Deserialize<PaystackInitializeResponse>(
        responseJson,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

    return result ?? throw new InvalidOperationException(
        "Unable to parse Paystack response.");
}

public async Task<PaystackVerifyResponse> VerifyTransaction(
    string reference)
{
    var response = await _httpClient.GetAsync(
        $"/transaction/verify/{reference}");

    response.EnsureSuccessStatusCode();

    var responseJson = await response.Content.ReadAsStringAsync();

    var result = JsonSerializer.Deserialize<PaystackVerifyResponse>(
        responseJson,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

    return result ?? throw new InvalidOperationException(
        "Unable to parse Paystack verification response.");
}
}