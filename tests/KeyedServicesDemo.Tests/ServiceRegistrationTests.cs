using KeyedServicesDemo.Abstractions;
using KeyedServicesDemo.Clients;
using KeyedServicesDemo.Extensions;
using KeyedServicesDemo.Processors;
using KeyedServicesDemo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KeyedServicesDemo.Tests;

/// <summary>
/// Validates that the <see cref="ServiceCollectionExtensions.AddKeyedStrategy{TInterface,TImplementation}"/>
/// extension method registers strategies under the correct keys and that the
/// DI container resolves only the requested strategy.
/// </summary>
public class ServiceRegistrationTests
{
    private static IServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddScoped<ICreditCardGateway, CreditCardGateway>();
        services.AddScoped<IPayPalClient, PayPalClient>();
        services.AddScoped<IBankingApi, BankingApi>();

        services.AddKeyedStrategy<IPaymentProcessor, CreditCardProcessor>();
        services.AddKeyedStrategy<IPaymentProcessor, PayPalProcessor>();
        services.AddKeyedStrategy<IPaymentProcessor, BankTransferProcessor>();

        services.AddScoped<PaymentService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void CreditCardProcessor_Key_IsExpectedValue()
    {
        Assert.Equal("creditcard", CreditCardProcessor.Key);
    }

    [Fact]
    public void PayPalProcessor_Key_IsExpectedValue()
    {
        Assert.Equal("paypal", PayPalProcessor.Key);
    }

    [Fact]
    public void BankTransferProcessor_Key_IsExpectedValue()
    {
        Assert.Equal("banktransfer", BankTransferProcessor.Key);
    }

    [Theory]
    [InlineData("creditcard", typeof(CreditCardProcessor))]
    [InlineData("paypal",     typeof(PayPalProcessor))]
    [InlineData("banktransfer", typeof(BankTransferProcessor))]
    public void KeyedService_ResolvesCorrectImplementation(string key, Type expectedType)
    {
        using var scope = BuildProvider().CreateScope();
        var processor = scope.ServiceProvider.GetRequiredKeyedService<IPaymentProcessor>(key);

        Assert.IsType(expectedType, processor);
    }

    [Fact]
    public void ResolveByKey_DoesNotInstantiateOtherStrategies()
    {
        // Arrange: replace the clients with spies that record construction.
        var constructedClients = new List<string>();

        var services = new ServiceCollection();
        services.AddScoped<ICreditCardGateway>(_ =>
        {
            constructedClients.Add(nameof(ICreditCardGateway));
            return new CreditCardGateway();
        });
        services.AddScoped<IPayPalClient>(_ =>
        {
            constructedClients.Add(nameof(IPayPalClient));
            return new PayPalClient();
        });
        services.AddScoped<IBankingApi>(_ =>
        {
            constructedClients.Add(nameof(IBankingApi));
            return new BankingApi();
        });

        services.AddKeyedStrategy<IPaymentProcessor, CreditCardProcessor>();
        services.AddKeyedStrategy<IPaymentProcessor, PayPalProcessor>();
        services.AddKeyedStrategy<IPaymentProcessor, BankTransferProcessor>();

        using var scope = services.BuildServiceProvider().CreateScope();

        // Act: resolve only the PayPal strategy.
        _ = scope.ServiceProvider.GetRequiredKeyedService<IPaymentProcessor>(PayPalProcessor.Key);

        // Assert: only the PayPal client was constructed; the others were not.
        Assert.Contains(nameof(IPayPalClient), constructedClients);
        Assert.DoesNotContain(nameof(ICreditCardGateway), constructedClients);
        Assert.DoesNotContain(nameof(IBankingApi), constructedClients);
    }

    [Fact]
    public async Task PaymentService_CreditCard_ReturnsSuccess()
    {
        using var scope = BuildProvider().CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PaymentService>();

        var result = await service.PayAsync(CreditCardProcessor.Key, "4111111111111111", 9.99m);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.TransactionId));
    }

    [Fact]
    public async Task PaymentService_PayPal_ReturnsSuccess()
    {
        using var scope = BuildProvider().CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PaymentService>();

        var result = await service.PayAsync(PayPalProcessor.Key, "buyer@example.com", 59.99m);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.TransactionId));
    }

    [Fact]
    public async Task PaymentService_BankTransfer_ReturnsSuccess()
    {
        using var scope = BuildProvider().CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PaymentService>();

        var result = await service.PayAsync(BankTransferProcessor.Key, "000987654321", 1_000m);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.TransactionId));
    }

    [Fact]
    public async Task PaymentService_UnknownKey_ThrowsInvalidOperationException()
    {
        using var scope = BuildProvider().CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PaymentService>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PayAsync("unknown", "recipient", 1m));
    }
}
