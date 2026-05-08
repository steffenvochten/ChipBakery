using System.Collections.Concurrent;

namespace Agents.Service.Workers;

public sealed record SupplierPersona(
    string Name,
    string SystemPrompt,
    string[] Keywords,
    decimal LowThreshold,
    decimal DefaultRestockQty,
    TimeSpan Interval);

public interface ISupplierManager
{
    IEnumerable<SupplierPersona> GetActiveSuppliers();
    bool RegisterSupplier(SupplierPersona persona);
    event Action<SupplierPersona>? OnSupplierRegistered;
}

public class SupplierManager : ISupplierManager
{
    private readonly ConcurrentDictionary<string, SupplierPersona> _suppliers = new();
    public event Action<SupplierPersona>? OnSupplierRegistered;

    public IEnumerable<SupplierPersona> GetActiveSuppliers() => _suppliers.Values;

    public bool RegisterSupplier(SupplierPersona persona)
    {
        if (_suppliers.TryAdd(persona.Name, persona))
        {
            OnSupplierRegistered?.Invoke(persona);
            return true;
        }
        return false;
    }
}
