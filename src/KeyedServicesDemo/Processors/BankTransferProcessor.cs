using KeyedServicesDemo.Abstractions;
using KeyedServicesDemo.Clients;

namespace KeyedServicesDemo.Processors;

/// <summary>
/// Payment processor strategy for bank-transfer transactions.
/// Its dependency on <see cref="IBankingApi"/> is only instantiated when
/// this specific strategy is resolved from the DI container by its key.
/// </summary>
public sealed class BankTransferProcessor : IPaymentProcessor, IKeyedStrategy
{
    // --- C# 11 static abstract interface member implementation ---
    public static string Key => "banktransfer";

    private readonly IBankingApi _bankingApi;

    public BankTransferProcessor(IBankingApi bankingApi)
    {
        _bankingApi = bankingApi;
    }

    public async Task<PaymentResult> ProcessAsync(string recipient, decimal amount)
    {
        var (success, transactionId) = await _bankingApi.InitiateTransferAsync(recipient, amount);
        return new PaymentResult(success, transactionId, success
            ? $"Bank transfer of {amount:C} initiated successfully."
            : "Bank transfer failed.");
    }
}
