namespace KeyedServicesDemo.Clients;

/// <summary>Abstraction for a banking / ACH transfer API.</summary>
public interface IBankingApi
{
    Task<(bool Success, string TransactionId)> InitiateTransferAsync(string accountNumber, decimal amount);
}

/// <summary>
/// Lightweight stub that simulates a bank transfer API.
/// In a real application this would integrate with an ACH or SWIFT provider.
/// </summary>
public sealed class BankingApi : IBankingApi
{
    public Task<(bool Success, string TransactionId)> InitiateTransferAsync(string accountNumber, decimal amount)
    {
        Console.WriteLine($"  [BankingApi] Initiating bank transfer of {amount:C} to account {accountNumber[^4..]}");
        return Task.FromResult((true, $"BT-{Guid.NewGuid():N}"[..12]));
    }
}
