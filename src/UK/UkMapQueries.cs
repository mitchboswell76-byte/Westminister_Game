using Westminster.Core;

namespace Westminster.UK;

public static class UkMapQueries
{
    public static UkRegion? GetRegionById(List<UkRegion> regions, string regionId)
    {
        return regions.SingleOrDefault(r => string.Equals(r.Id, regionId, StringComparison.Ordinal));
    }

    public static List<ConstituencyMapBinding> ConstituenciesByRegion(List<ConstituencyMapBinding> bindings, string regionId)
    {
        return bindings
            .Where(b => string.Equals(b.RegionId, regionId, StringComparison.Ordinal))
            .OrderBy(b => b.ConstituencyId, StringComparer.Ordinal)
            .ToList();
    }

    public static bool ValidateEveryConstituencyHasRegionBinding(List<Constituency> constituencies, List<ConstituencyMapBinding> bindings)
    {
        var bindingSet = new HashSet<string>(bindings.Select(b => b.ConstituencyId), StringComparer.Ordinal);
        return constituencies.All(c => bindingSet.Contains(c.Id));
    }

    public static bool ValidateEveryPopRegionExistsInUkRegions(List<Pop> pops, List<UkRegion> regions)
    {
        var regionSet = new HashSet<string>(regions.Select(r => r.Id), StringComparer.Ordinal);
        return pops.All(p => regionSet.Contains(p.RegionId));
    }

    public static Dictionary<string, int> CountConstituenciesByRegion(List<ConstituencyMapBinding> bindings)
    {
        return bindings
            .GroupBy(b => b.RegionId, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
    }
}
