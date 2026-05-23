using Westminster.Core;
using Westminster.Simulation;
using Westminster.UK;

namespace Westminster.UI.Map;

public static class UkMapViewModelBuilder
{
    public static UkMapViewModel Build(
        GameState state,
        MapFeatureCollection featureCollection,
        string? selectedRegionId = null,
        string? selectedConstituencyId = null)
    {
        var validationMessages = new List<string>();

        var topology = state.MapTopologies.SingleOrDefault(x => string.Equals(x.Id, featureCollection.TopologyId, StringComparison.Ordinal));
        if (topology is null)
        {
            validationMessages.Add($"Missing topology metadata for topology id {featureCollection.TopologyId}.");
        }

        var hasValidFeatures = UkTopologyValidator.ValidateFeatures(featureCollection.Features, out var featureValidationMessage);
        if (!hasValidFeatures)
        {
            validationMessages.Add(featureValidationMessage);
        }

        var hasValidBindings = UkTopologyValidator.ValidateBindingsAgainstFeatures(state.ConstituencyMapBindings, featureCollection.Features, out var bindingValidationMessage);
        if (!hasValidBindings)
        {
            validationMessages.Add(bindingValidationMessage);
        }

        var latestElectionResult = state.ElectionResults
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id, StringComparer.Ordinal)
            .FirstOrDefault();

        var winnersByConstituency = BuildWinnersByConstituency(latestElectionResult);

        var constituencyCounts = UkMapQueries.CountConstituenciesByRegion(state.ConstituencyMapBindings);
        var regions = state.UkRegions
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .Select((region, index) =>
            {
                var count = constituencyCounts.TryGetValue(region.Id, out var value) ? value : 0;
                return new UkMapRegionView(
                    region.Id,
                    region.Name,
                    region.Country,
                    count,
                    string.Equals(region.Id, selectedRegionId, StringComparison.Ordinal),
                    index);
            })
            .ToList();

        var features = featureCollection.Features
            .OrderBy(x => x.Id, StringComparer.Ordinal)
            .Select((feature, index) => new UkMapFeatureView(
                feature.Id,
                feature.ConstituencyId,
                feature.RegionId,
                feature.Name,
                feature.TopojsonObjectId,
                winnersByConstituency.TryGetValue(feature.ConstituencyId, out var winnerPartyId) ? winnerPartyId : null,
                string.Equals(feature.ConstituencyId, selectedConstituencyId, StringComparison.Ordinal),
                index))
            .ToList();

        return new UkMapViewModel(
            topology?.Id ?? featureCollection.TopologyId,
            topology?.Name ?? "Unknown topology",
            topology?.Source ?? "unknown",
            validationMessages.Count == 0,
            validationMessages,
            regions,
            features,
            selectedRegionId,
            selectedConstituencyId);
    }

    private static Dictionary<string, string> BuildWinnersByConstituency(ElectionResult? electionResult)
    {
        var winners = new Dictionary<string, string>(StringComparer.Ordinal);
        if (electionResult is null)
        {
            return winners;
        }

        foreach (var result in electionResult.ConstituencyResults.OrderBy(x => x.ConstituencyId, StringComparer.Ordinal))
        {
            winners[result.ConstituencyId] = result.WinnerPartyId;
        }

        return winners;
    }
}
