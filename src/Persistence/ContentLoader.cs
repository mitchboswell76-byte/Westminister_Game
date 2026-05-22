using System.Text.Json;
using Westminster.Core;

namespace Westminster.Persistence;

public static class ContentLoader
{
    public static List<PolicyLever> LoadPolicyLevers(string contentRoot)
    {
        var dir = Path.Combine(contentRoot, "content", "policies");
        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException(dir);
        var list = new List<PolicyLever>();
        foreach (var file in Directory.GetFiles(dir, "*.json").OrderBy(x => x, StringComparer.Ordinal))
        {
            var raw = File.ReadAllLines(file).Where(l => !l.TrimStart().StartsWith("//", StringComparison.Ordinal)).ToArray();
            var parsed = JsonSerializer.Deserialize<List<PolicyLever>>(string.Join("\n", raw), JsonSupport.Options) ?? throw new InvalidOperationException($"Invalid policy json: {file}");
            list.AddRange(parsed);
        }

        var dup = list.GroupBy(x => x.Id).FirstOrDefault(g => g.Count() > 1);
        if (dup is not null) throw new InvalidOperationException($"Duplicate lever id: {dup.Key}");
        foreach (var p in list)
        {
            if (string.IsNullOrWhiteSpace(p.PhaseTag)) throw new InvalidOperationException($"Missing phase tag: {p.Id}");
            if (p.Effects is null || p.Effects.Count == 0) throw new InvalidOperationException($"Missing effects: {p.Id}");
            if (p.Min > p.Default || p.Default > p.Max) throw new InvalidOperationException($"Default outside min/max: {p.Id}");
        }

        return list.OrderBy(x => x.Id, StringComparer.Ordinal).ToList();
    }

    public static List<PolicyLever> MvpOnly(IEnumerable<PolicyLever> levers) => levers.Where(x => x.PhaseTag == "MVP").OrderBy(x => x.Id, StringComparer.Ordinal).ToList();
}
