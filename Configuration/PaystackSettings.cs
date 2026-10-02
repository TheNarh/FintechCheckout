namespace FintechCheckout.Configuration;

public class PaystackSettings
{
    public string SecretKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.paystack.co";

    public string CallbackUrl { get; set; } =
        "https://fintech-checkout.vercel.app/payment-result";
}