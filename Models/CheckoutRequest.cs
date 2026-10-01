using System.ComponentModel.DataAnnotations;

namespace FintechCheckout.Models;

public class CheckoutRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}