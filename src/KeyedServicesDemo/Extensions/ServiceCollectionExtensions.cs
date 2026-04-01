using KeyedServicesDemo.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KeyedServicesDemo.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> that leverage
/// C# 11 static abstract interface members to register keyed strategies
/// without magic strings.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TImplementation"/> as a keyed implementation of
    /// <typeparamref name="TInterface"/> in the DI container.
    ///
    /// The registration key is read from <c>TImplementation.Key</c> – a <em>static</em>
    /// property declared on the concrete type through the
    /// <see cref="IKeyedStrategy"/> interface's static abstract member.
    /// This means the key is part of the type's compile-time contract: there are
    /// no magic strings at the call-site, and a missing <c>Key</c> implementation
    /// is a compile error rather than a runtime surprise.
    /// </summary>
    /// <typeparam name="TInterface">The service type to register.</typeparam>
    /// <typeparam name="TImplementation">
    /// The concrete implementation. Must implement both <typeparamref name="TInterface"/>
    /// and <see cref="IKeyedStrategy"/> so that <c>TImplementation.Key</c> is accessible.
    /// </typeparam>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="lifetime">
    /// Optional service lifetime; defaults to <see cref="ServiceLifetime.Scoped"/>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddKeyedStrategy<TInterface, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TInterface : class
        where TImplementation : class, TInterface, IKeyedStrategy
    {
        // TImplementation.Key is resolvable here because the generic constraint
        // 'where TImplementation : IKeyedStrategy' gives the compiler proof that
        // TImplementation satisfies the static abstract member contract.
        services.Add(new ServiceDescriptor(
            typeof(TInterface),
            TImplementation.Key,
            typeof(TImplementation),
            lifetime));

        return services;
    }
}
