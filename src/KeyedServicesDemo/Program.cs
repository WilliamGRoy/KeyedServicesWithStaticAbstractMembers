using KeyedServicesDemo.Clients;
using KeyedServicesDemo.Extensions;
using KeyedServicesDemo.Abstractions;
using KeyedServicesDemo.Processors;
using KeyedServicesDemo.Services;
using Microsoft.Extensions.DependencyInjection;

// ---------------------------------------------------------------------------
// Demo: Keyed Services + C# 11 Static Abstract Interface Members
//       implementing the Strategy Pattern
//
// Goal: resolve *only* the required payment strategy (and its dependencies)
//       without instantiating all registered strategies up-front.
// ---------------------------------------------------------------------------

var services = new ServiceCollection();

// --- Register client dependencies (one per payment provider) ---------------
// In a real application each of these might pull in a heavyweight provider SDK.
services.AddScoped<ICreditCardGateway, CreditCardGateway>();
services.AddScoped<IPayPalClient, PayPalClient>();
services.AddScoped<IBankingApi, BankingApi>();

// --- Register keyed strategies ─────────────────────────────────────────────
// AddKeyedStrategy<TInterface, TImplementation> reads the key directly from
// TImplementation.Key (the static abstract member), so there is no magic
// string at the call-site.  A typo in the key becomes a compile error.
services.AddKeyedStrategy<IPaymentProcessor, CreditCardProcessor>();
services.AddKeyedStrategy<IPaymentProcessor, PayPalProcessor>();
services.AddKeyedStrategy<IPaymentProcessor, BankTransferProcessor>();

// --- Register the orchestrating service ------------------------------------
services.AddScoped<PaymentService>();

await using var rootProvider = services.BuildServiceProvider();

// Use a scope so scoped services are properly disposed after each scenario.
Console.WriteLine("=== Keyed Services + Static Abstract Members Demo ===\n");

var scenarios = new[]
{
    (Method: CreditCardProcessor.Key,    Recipient: "4111111111111111", Amount: 49.99m),
    (Method: PayPalProcessor.Key,        Recipient: "buyer@example.com", Amount: 120.00m),
    (Method: BankTransferProcessor.Key,  Recipient: "000123456789",      Amount: 5_000.00m),
};

foreach (var (method, recipient, amount) in scenarios)
{
    await using var scope = rootProvider.CreateAsyncScope();
    var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();

    Console.WriteLine($"Processing {method} payment of {amount:C} for '{recipient}'…");
    var result = await paymentService.PayAsync(method, recipient, amount);
    Console.WriteLine($"  Result : {(result.Success ? "✓" : "✗")} {result.Message}");
    Console.WriteLine($"  Txn ID : {result.TransactionId}\n");
}

Console.WriteLine("=== Demo complete ===");
