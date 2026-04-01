using KeyedServicesDemo.Abstractions;
using KeyedServicesDemo.Clients;

namespace KeyedServicesDemo.Processors;

/// <summary>
/// Payment processor strategy for credit-card transactions.
/// Its dependency on <see cref="ICreditCardGateway"/> is only instantiated when
/// this specific strategy is resolved from the DI container by its key.
/// </summary>
public sealed class CreditCardProcessor : IPaymentProcessor, IKeyedStrategy
{
    // --- C# 11 static abstract interface member implementation ---
    public static string Key => "creditcard";

    private readonly ICreditCardGateway _gateway;

    public CreditCardProcessor(ICreditCardGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<PaymentResult> ProcessAsync(string recipient, decimal amount)
    {
        var (success, transactionId) = await _gateway.ChargeAsync(recipient, amount);
        return new PaymentResult(success, transactionId, success
            ? $"Credit-card charge of {amount:C} succeeded."
            : "Credit-card charge failed.");
    }
}
