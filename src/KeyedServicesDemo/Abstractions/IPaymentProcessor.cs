namespace KeyedServicesDemo.Abstractions;

/// <summary>
/// Strategy interface for processing payments.
/// Concrete implementations additionally implement <see cref="IKeyedStrategy"/>
/// to expose a static key, enabling the DI extension method to register them
/// without magic strings (see
/// <see cref="Extensions.ServiceCollectionExtensions.AddKeyedStrategy{TInterface,TImplementation}"/>).
/// </summary>
public interface IPaymentProcessor
{
    /// <summary>Processes a payment of the given <paramref name="amount"/>.</summary>
    /// <param name="recipient">Email address or account identifier of the payee.</param>
    /// <param name="amount">Amount (in the account's base currency) to charge.</param>
    /// <returns>A <see cref="PaymentResult"/> describing the outcome.</returns>
    Task<PaymentResult> ProcessAsync(string recipient, decimal amount);
}

/// <summary>Represents the outcome of a payment operation.</summary>
/// <param name="Success">Whether the payment succeeded.</param>
/// <param name="TransactionId">Provider-assigned transaction identifier.</param>
/// <param name="Message">Human-readable status message.</param>
public sealed record PaymentResult(bool Success, string TransactionId, string Message);
