namespace KeyedServicesDemo.Clients;

/// <summary>Abstraction for a credit-card payment gateway.</summary>
public interface ICreditCardGateway
{
    Task<(bool Success, string TransactionId)> ChargeAsync(string cardNumber, decimal amount);
}

/// <summary>
/// Lightweight stub that simulates a real credit-card gateway.
/// In a real application this would be backed by a provider SDK (e.g. Stripe, Braintree).
/// </summary>
public sealed class CreditCardGateway : ICreditCardGateway
{
    public Task<(bool Success, string TransactionId)> ChargeAsync(string cardNumber, decimal amount)
    {
        Console.WriteLine($"  [CreditCardGateway] Charging {amount:C} to card ending {cardNumber[^4..]}");
        return Task.FromResult((true, $"CC-{Guid.NewGuid():N}"[..12]));
    }
}
