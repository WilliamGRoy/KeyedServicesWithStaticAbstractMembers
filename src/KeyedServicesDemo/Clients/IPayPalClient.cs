namespace KeyedServicesDemo.Clients;

/// <summary>Abstraction for the PayPal REST client.</summary>
public interface IPayPalClient
{
    Task<(bool Success, string TransactionId)> CreateOrderAsync(string email, decimal amount);
}

/// <summary>
/// Lightweight stub that simulates the PayPal REST client.
/// In a real application this would call the PayPal Orders API.
/// </summary>
public sealed class PayPalClient : IPayPalClient
{
    public Task<(bool Success, string TransactionId)> CreateOrderAsync(string email, decimal amount)
    {
        Console.WriteLine($"  [PayPalClient] Creating PayPal order for {email}, amount {amount:C}");
        return Task.FromResult((true, $"PP-{Guid.NewGuid():N}"[..12]));
    }
}
