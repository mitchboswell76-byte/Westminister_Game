namespace Westminster.UI.Map;

public record UkMapRegionView(
    string RegionId,
    string Name,
    string Country,
    int ConstituencyCount,
    bool IsSelected,
    int DisplayOrder);

public record UkMapFeatureView(
    string FeatureId,
    string ConstituencyId,
    string RegionId,
    string Name,
    string TopojsonObjectId,
    string? WinnerPartyId,
    bool IsSelected,
    int DisplayOrder);

public record UkMapViewModel(
    string TopologyId,
    string TopologyName,
    string TopologySource,
    bool IsValid,
    List<string> ValidationMessages,
    List<UkMapRegionView> Regions,
    List<UkMapFeatureView> Features,
    string? SelectedRegionId,
    string? SelectedConstituencyId);
