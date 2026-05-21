using Westminster.Core;
using Westminster.Simulation;

namespace Westminster.Policy;

public static class PolicyEngine
{
    // TODO P2: three-phase enactment
    public static void ApplyChange(GameState s, string leverId, double newValue)
    {
        var lever = s.Policies.FirstOrDefault(p => p.Id == leverId) ?? throw new InvalidOperationException($"Unknown lever: {leverId}");
        if (newValue < lever.Min || newValue > lever.Max) throw new InvalidOperationException("Policy value outside min/max.");
        var steps = (newValue - lever.Min) / lever.Step;
        if (Math.Abs(steps - Math.Round(steps)) > 1e-9) throw new InvalidOperationException("Policy value violates step.");

        var enabled = lever.EnabledByIdeologies.Contains("*") || lever.EnabledByIdeologies.Contains(s.Player.IdeologyId);
        if (!enabled) throw new InvalidOperationException("Policy not enabled for ideology.");
        if (lever.DisabledByGovernmentTypes.Contains(s.GovernmentType)) throw new InvalidOperationException("Policy disabled by government type.");

        lever = lever with { CurrentValue = newValue };
        var idx = s.Policies.FindIndex(p => p.Id == leverId);
        s.Policies[idx] = lever;
    }

    public static void TickMonthly(GameState s)
    {
        s.MetricsLedger.Clear();
        foreach (var lever in s.Policies.OrderBy(p => p.Id, StringComparer.Ordinal))
        {
            foreach (var effect in lever.Effects)
            {
                s.MetricsLedger.Add(effect.Target, effect.Evaluate(lever.CurrentValue));
            }
        }
    }

    public static Dictionary<string, double> GetFactionReactionDeltas(GameState s)
    {
        var result = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var lever in s.Policies.OrderBy(p => p.Id, StringComparer.Ordinal))
        {
            foreach (var kv in lever.FactionReactions.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                result[kv.Key] = (result.TryGetValue(kv.Key, out var old) ? old : 0d) + kv.Value;
            }
        }

        return result;
    }
}
