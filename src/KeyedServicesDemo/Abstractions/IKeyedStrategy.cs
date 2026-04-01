namespace KeyedServicesDemo.Abstractions;

/// <summary>
/// Defines a contract that exposes a static key used to identify a strategy
/// implementation within the dependency injection container.
///
/// By combining this interface with C# 11 static abstract interface members,
/// the key is part of the type's compile-time contract rather than a runtime
/// magic string, and it can be consumed in generic extension methods
/// (see <see cref="Extensions.ServiceCollectionExtensions.AddKeyedStrategy{TInterface,TImplementation}"/>).
/// </summary>
public interface IKeyedStrategy
{
    /// <summary>Gets the unique key that identifies this strategy.</summary>
    static abstract string Key { get; }
}
