# KeyedServicesWithStaticAbstractMembers

A demonstration of the **Strategy Pattern** using two modern .NET/C# features together:

| Feature | Version |
|---|---|
| [Keyed Services](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8/runtime#keyed-di-services) | .NET 8 |
| [Static Abstract Interface Members](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#static-abstract-and-virtual-members-in-interfaces) | C# 11 |

## The Problem

When multiple strategy implementations are registered with the DI container the
traditional approach requires injecting **all** of them (e.g. via
`IEnumerable<IStrategy>`) and picking one at runtime.  This forces every
implementation – and **all of its transitive dependencies** – to be constructed
even when only one is ever needed.

## The Solution

1. **Keyed Services** – register each strategy under a unique key so the
   container instantiates only the strategy that is actually needed.
2. **Static Abstract Interface Members** – encode the key as a *static*
   property on each concrete type, accessed through a generic constraint, so
   there are no magic strings at the registration site and a typo becomes a
   compile error.

## Project Structure

```
src/
└── KeyedServicesDemo/
    ├── Abstractions/
    │   ├── IKeyedStrategy.cs          ← interface with static abstract Key
    │   └── IPaymentProcessor.cs       ← strategy interface + PaymentResult record
    ├── Clients/
    │   ├── ICreditCardGateway.cs      ← provider abstraction + stub
    │   ├── IPayPalClient.cs           ← provider abstraction + stub
    │   └── IBankingApi.cs             ← provider abstraction + stub
    ├── Extensions/
    │   └── ServiceCollectionExtensions.cs  ← AddKeyedStrategy<TInterface, TImplementation>()
    ├── Processors/
    │   ├── CreditCardProcessor.cs     ← Key = "creditcard"
    │   ├── PayPalProcessor.cs         ← Key = "paypal"
    │   └── BankTransferProcessor.cs   ← Key = "banktransfer"
    ├── Services/
    │   └── PaymentService.cs          ← resolves strategy by key at runtime
    └── Program.cs                     ← wires everything together
tests/
└── KeyedServicesDemo.Tests/
    └── ServiceRegistrationTests.cs    ← validates keys, resolution, lazy-loading
```

## Key Concepts

### `IKeyedStrategy` – the static abstract member

```csharp
public interface IKeyedStrategy
{
    static abstract string Key { get; }
}
```

### Concrete strategy – implements both interfaces independently

```csharp
public sealed class CreditCardProcessor : IPaymentProcessor, IKeyedStrategy
{
    public static string Key => "creditcard";   // ← satisfies IKeyedStrategy
    // ...
}
```

### `AddKeyedStrategy` – magic-string-free registration

```csharp
public static IServiceCollection AddKeyedStrategy<TInterface, TImplementation>(
    this IServiceCollection services,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
    where TInterface : class
    where TImplementation : class, TInterface, IKeyedStrategy
{
    // TImplementation.Key is accessible here because of the IKeyedStrategy constraint.
    services.Add(new ServiceDescriptor(
        typeof(TInterface), TImplementation.Key, typeof(TImplementation), lifetime));
    return services;
}
```

### Registration – no magic strings

```csharp
services.AddKeyedStrategy<IPaymentProcessor, CreditCardProcessor>();
services.AddKeyedStrategy<IPaymentProcessor, PayPalProcessor>();
services.AddKeyedStrategy<IPaymentProcessor, BankTransferProcessor>();
```

### Runtime resolution – only the required strategy is instantiated

```csharp
var processor = serviceProvider.GetRequiredKeyedService<IPaymentProcessor>(paymentMethod);
```

## Building & Running

```bash
# Build everything
dotnet build

# Run the console demo
dotnet run --project src/KeyedServicesDemo

# Run the tests
dotnet test
```

## Sample Output

```
=== Keyed Services + Static Abstract Members Demo ===

Processing creditcard payment of $49.99 for '4111111111111111'…
  [CreditCardGateway] Charging $49.99 to card ending 1111
  Result : ✓ Credit-card charge of $49.99 succeeded.
  Txn ID : CC-21355ff60

Processing paypal payment of $120.00 for 'buyer@example.com'…
  [PayPalClient] Creating PayPal order for buyer@example.com, amount $120.00
  Result : ✓ PayPal payment of $120.00 succeeded.
  Txn ID : PP-9fa166037

Processing banktransfer payment of $5,000.00 for '000123456789'…
  [BankingApi] Initiating bank transfer of $5,000.00 to account 6789
  Result : ✓ Bank transfer of $5,000.00 initiated successfully.
  Txn ID : BT-6cf3d3cd7

=== Demo complete ===
```
