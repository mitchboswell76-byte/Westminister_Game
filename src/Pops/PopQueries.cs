using Westminster.Core;

namespace Westminster.Pops;

public static class PopQueries
{
    public static long TotalRepresentedPopulation(IEnumerable<Pop> pops) => pops.Sum(p => p.Size);

    public static Dictionary<string, int> PopCountByRegion(IEnumerable<Pop> pops) =>
        pops.GroupBy(p => p.RegionId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

    public static Dictionary<string, long> RepresentedPopulationByRegion(IEnumerable<Pop> pops) =>
        pops.GroupBy(p => p.RegionId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Sum(x => x.Size), StringComparer.Ordinal);

    public static Dictionary<string, long> RepresentedPopulationByStratum(IEnumerable<Pop> pops) =>
        pops.GroupBy(p => p.Stratum, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Sum(x => x.Size), StringComparer.Ordinal);

    public static double AverageEngagement(IEnumerable<Pop> pops)
    {
        var materialized = pops as IList<Pop> ?? pops.ToList();
        return materialized.Count == 0 ? 0d : materialized.Average(p => p.Engagement);
    }

    public static double AverageIdeologyAxis(IEnumerable<Pop> pops, string axis)
    {
        var values = pops.Where(p => p.IdeologyVector.TryGetValue(axis, out _)).Select(p => p.IdeologyVector[axis]).ToList();
        return values.Count == 0 ? 0d : values.Average();
    }

    public static Dictionary<string, double> AverageIdeologyAxisByRegion(IEnumerable<Pop> pops, string axis)
    {
        return pops
            .Where(p => p.IdeologyVector.ContainsKey(axis))
            .GroupBy(p => p.RegionId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Average(p => p.IdeologyVector[axis]), StringComparer.Ordinal);
    }
}
