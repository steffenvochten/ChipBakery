namespace Agents.Service;

public sealed class SupplierPersonaConfig
{
    public string   Name             { get; set; } = string.Empty;
    public string   SystemPrompt     { get; set; } = string.Empty;
    public string[] Keywords         { get; set; } = [];
    public decimal  LowThreshold     { get; set; }
    public decimal  DefaultRestockQty{ get; set; }
    public int      IntervalSeconds  { get; set; } = 30;
}

public sealed class SupplierAgentOptions
{
    public List<SupplierPersonaConfig> Suppliers { get; set; } = [];
}
