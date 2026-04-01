using KeyedServicesDemo.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KeyedServicesDemo.Services;

/// <summary>
/// Orchestrates payment processing by selecting the appropriate
/// <see cref="IPaymentProcessor"/> strategy at runtime via the keyed DI container.
///
/// <para>
/// Key design point: only the strategy that is actually needed gets
/// instantiated on each call.  The other strategies – together with
/// <em>all of their transitive dependencies</em> – are never constructed,
/// which keeps memory pressure and startup time low when each strategy
/// carries heavyweight provider SDKs.
/// </para>
/// </summary>
public sealed class PaymentService
{
    private readonly IKeyedServiceProvider _serviceProvider;

    public PaymentService(IServiceProvider serviceProvider)
    {
        // In .NET 8+, the scoped IServiceProvider resolved from the DI container
        // is a ServiceProviderEngineScope which implements IKeyedServiceProvider.
        _serviceProvider = (IKeyedServiceProvider)serviceProvider;
    }

    /// <summary>
    /// Processes a payment using the strategy registered under
    /// <paramref name="paymentMethod"/>.
    /// </summary>
    /// <param name="paymentMethod">
    /// The key that identifies the desired strategy
    /// (e.g. <c>"creditcard"</c>, <c>"paypal"</c>, <c>"banktransfer"</c>).
    /// </param>
    /// <param name="recipient">Payee identifier (card number, email, account).</param>
    /// <param name="amount">Amount to charge.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no strategy is registered for <paramref name="paymentMethod"/>.
    /// </exception>
    public async Task<PaymentResult> PayAsync(string paymentMethod, string recipient, decimal amount)
    {
        var processor = _serviceProvider.GetRequiredKeyedService<IPaymentProcessor>(paymentMethod);
        return await processor.ProcessAsync(recipient, amount);
    }
}
