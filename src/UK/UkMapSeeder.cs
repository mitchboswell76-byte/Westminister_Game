using Westminster.Core;

namespace Westminster.UK;

public static class UkMapSeeder
{
    public static List<MapTopologyMetadata> CreateMvpTopologies()
    {
        return new List<MapTopologyMetadata>
        {
            new(
                "topology_mvp_constituencies",
                "MVP Constituency Placeholder Topology",
                "fixture_placeholder_until_step_8b",
                "epsg:4326",
                12,
                "mvp_step8a")
        };
    }

    public static List<ConstituencyMapBinding> CreateMvpBindings(List<Constituency> constituencies)
    {
        var bindings = new List<ConstituencyMapBinding>();
        foreach (var constituency in constituencies.OrderBy(c => c.Id, StringComparer.Ordinal))
        {
            bindings.Add(new ConstituencyMapBinding(
                constituency.Id,
                constituency.Region,
                $"mvp_topo_{constituency.Id}",
                string.IsNullOrWhiteSpace(constituency.LadId) ? $"lad_placeholder_{constituency.Id}" : constituency.LadId));
        }

        return bindings;
    }
}
