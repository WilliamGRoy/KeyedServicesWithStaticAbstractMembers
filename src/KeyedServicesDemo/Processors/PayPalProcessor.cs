using KeyedServicesDemo.Abstractions;
using KeyedServicesDemo.Clients;

namespace KeyedServicesDemo.Processors;

/// <summary>
/// Payment processor strategy for PayPal transactions.
/// Its dependency on <see cref="IPayPalClient"/> is only instantiated when
/// this specific strategy is resolved from the DI container by its key.
/// </summary>
public sealed class PayPalProcessor : IPaymentProcessor, IKeyedStrategy
{
    // --- C# 11 static abstract interface member implementation ---
    public static string Key => "paypal";

    private readonly IPayPalClient _client;

    public PayPalProcessor(IPayPalClient client)
    {
        _client = client;
    }

    public async Task<PaymentResult> ProcessAsync(string recipient, decimal amount)
    {
        var (success, transactionId) = await _client.CreateOrderAsync(recipient, amount);
        return new PaymentResult(success, transactionId, success
            ? $"PayPal payment of {amount:C} succeeded."
            : "PayPal payment failed.");
    }
}
