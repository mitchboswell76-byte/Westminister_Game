namespace Westminster.UI.Map;

public record UkMapRegionRow(
    string RegionId,
    string Name,
    string Country,
    int ConstituencyCount,
    bool IsSelected,
    int DisplayOrder);

public record UkMapFeatureRow(
    string FeatureId,
    string ConstituencyId,
    string RegionId,
    string Name,
    string WinnerPartyLabel,
    bool IsSelected,
    int DisplayOrder);

public record UkMapPresentation(
    List<UkMapRegionRow> RegionRows,
    List<UkMapFeatureRow> FeatureRows,
    string? SelectedRegionId,
    string? SelectedConstituencyId,
    string? SelectedFeatureId,
    string LatestWinnerPartyLabel);

public static class UkMapPresentationBuilder
{
    private const string UnknownWinnerLabel = "No election result";

    public static UkMapPresentation Build(
        UkMapViewModel viewModel,
        string? selectedRegionId = null,
        string? selectedConstituencyId = null,
        string? selectedFeatureId = null)
    {
        var resolvedRegionId = selectedRegionId ?? viewModel.SelectedRegionId;
        var resolvedConstituencyId = selectedConstituencyId ?? viewModel.SelectedConstituencyId;

        var regionRows = viewModel.Regions
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.RegionId, StringComparer.Ordinal)
            .Select(x => new UkMapRegionRow(
                x.RegionId,
                x.Name,
                x.Country,
                x.ConstituencyCount,
                string.Equals(x.RegionId, resolvedRegionId, StringComparison.Ordinal),
                x.DisplayOrder))
            .ToList();

        var featureRows = viewModel.Features
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.FeatureId, StringComparer.Ordinal)
            .Select(x => new UkMapFeatureRow(
                x.FeatureId,
                x.ConstituencyId,
                x.RegionId,
                x.Name,
                BuildWinnerPartyLabel(x.WinnerPartyId),
                string.Equals(x.ConstituencyId, resolvedConstituencyId, StringComparison.Ordinal)
                    || string.Equals(x.FeatureId, selectedFeatureId, StringComparison.Ordinal),
                x.DisplayOrder))
            .ToList();

        var latestWinnerPartyLabel = featureRows
            .Select(x => x.WinnerPartyLabel)
            .FirstOrDefault(x => !string.Equals(x, UnknownWinnerLabel, StringComparison.Ordinal))
            ?? UnknownWinnerLabel;

        return new UkMapPresentation(
            regionRows,
            featureRows,
            resolvedRegionId,
            resolvedConstituencyId,
            selectedFeatureId,
            latestWinnerPartyLabel);
    }

    private static string BuildWinnerPartyLabel(string? winnerPartyId)
    {
        if (string.IsNullOrWhiteSpace(winnerPartyId))
        {
            return UnknownWinnerLabel;
        }

        return $"Winner: {winnerPartyId}";
    }
}
