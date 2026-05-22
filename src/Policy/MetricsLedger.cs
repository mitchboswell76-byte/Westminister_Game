namespace Westminster.Policy;

public sealed class MetricsLedger
{
    private readonly Dictionary<string, double> _metrics = new(StringComparer.Ordinal);

    public void Add(string target, double delta)
    {
        _metrics[target] = Get(target) + delta;
    }

    public double Get(string target) => _metrics.TryGetValue(target, out var value) ? value : 0d;

    public Dictionary<string, double> Snapshot() => _metrics.OrderBy(kv => kv.Key, StringComparer.Ordinal).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);

    public void Clear() => _metrics.Clear();
}
