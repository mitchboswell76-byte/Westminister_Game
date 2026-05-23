using Westminster.Core;
using Westminster.Election;
using Westminster.Simulation;
using Westminster.UI.Map;
using Westminster.UK;
using GameCharacter = Westminster.Core.Character;
using Xunit;

namespace Westminster.Tests;

public class UkMapPresentationTests
{
    [Fact]
    public void UkMapPresentation_BuildsRegionRows()
    {
        var state = BuildMapState();
        var viewModel = BuildViewModel(state);

        var presentation = UkMapPresentationBuilder.Build(viewModel);

        Assert.Equal(viewModel.Regions.Count, presentation.RegionRows.Count);
    }

    [Fact]
    public void UkMapPresentation_BuildsFeatureRows()
    {
        var state = BuildMapState();
        var viewModel = BuildViewModel(state);

        var presentation = UkMapPresentationBuilder.Build(viewModel);

        Assert.Equal(viewModel.Features.Count, presentation.FeatureRows.Count);
    }

    [Fact]
    public void UkMapPresentation_UsesElectionWinnerLabels()
    {
        var state = BuildMapState();
        state.ElectionResults.Add(ElectionSystem.ResolveFptpElection(state, new GameRng(77UL)));
        var viewModel = BuildViewModel(state);

        var presentation = UkMapPresentationBuilder.Build(viewModel);

        Assert.Contains(presentation.FeatureRows, x => x.WinnerPartyLabel.StartsWith("Winner:", StringComparison.Ordinal));
    }

    [Fact]
    public void UkMapPresentation_CanSelectRegion()
    {
        var state = BuildMapState();
        var viewModel = BuildViewModel(state);
        var selectedRegionId = viewModel.Regions[0].RegionId;

        var presentation = UkMapPresentationBuilder.Build(viewModel, selectedRegionId: selectedRegionId);

        Assert.Equal(selectedRegionId, presentation.SelectedRegionId);
        Assert.Contains(presentation.RegionRows, x => x.RegionId == selectedRegionId && x.IsSelected);
    }

    [Fact]
    public void UkMapPresentation_CanSelectConstituency()
    {
        var state = BuildMapState();
        var viewModel = BuildViewModel(state);
        var selectedConstituencyId = viewModel.Features[0].ConstituencyId;

        var presentation = UkMapPresentationBuilder.Build(viewModel, selectedConstituencyId: selectedConstituencyId);

        Assert.Equal(selectedConstituencyId, presentation.SelectedConstituencyId);
        Assert.Contains(presentation.FeatureRows, x => x.ConstituencyId == selectedConstituencyId && x.IsSelected);
    }

    [Fact]
    public void UkMapPresentation_IsDeterministic()
    {
        var stateA = BuildMapState();
        var stateB = BuildMapState();
        stateA.ElectionResults.Add(ElectionSystem.ResolveFptpElection(stateA, new GameRng(500UL)));
        stateB.ElectionResults.Add(ElectionSystem.ResolveFptpElection(stateB, new GameRng(500UL)));

        var presentationA = UkMapPresentationBuilder.Build(BuildViewModel(stateA), "region_london", "const_london_central", "feat_london_central");
        var presentationB = UkMapPresentationBuilder.Build(BuildViewModel(stateB), "region_london", "const_london_central", "feat_london_central");

        Assert.Equal(Canonicalize(presentationA), Canonicalize(presentationB));
    }

    [Fact]
    public void UkMapPresentation_HandlesMissingElectionResults()
    {
        var state = BuildMapState();
        var viewModel = BuildViewModel(state);

        var presentation = UkMapPresentationBuilder.Build(viewModel);

        Assert.Equal("No election result", presentation.LatestWinnerPartyLabel);
        Assert.All(presentation.FeatureRows, row => Assert.Equal("No election result", row.WinnerPartyLabel));
    }

    private static GameState BuildMapState()
    {
        var state = new GameState(new DateOnly(2026, 1, 1), BuildPlayerCharacter());
        state.Constituencies.AddRange(ElectionQueries.CreateMvpConstituencies());
        state.UkRegions.AddRange(UkRegionSeeder.CreateMvpRegions());
        state.ConstituencyMapBindings.AddRange(UkMapSeeder.CreateMvpBindings(state.Constituencies));
        state.MapTopologies.AddRange(UkTopologyLoader.LoadTopologyMetadata(FindRepositoryRoot()));
        return state;
    }

    private static UkMapViewModel BuildViewModel(GameState state)
    {
        var collection = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());
        return UkMapViewModelBuilder.Build(state, collection);
    }

    private static string Canonicalize(UkMapPresentation presentation)
    {
        var regionRows = presentation.RegionRows
            .OrderBy(x => x.RegionId, StringComparer.Ordinal)
            .Select(x => $"{x.RegionId}:{x.Name}:{x.ConstituencyCount}:{x.IsSelected}:{x.DisplayOrder}");

        var featureRows = presentation.FeatureRows
            .OrderBy(x => x.FeatureId, StringComparer.Ordinal)
            .Select(x => $"{x.FeatureId}:{x.ConstituencyId}:{x.RegionId}:{x.WinnerPartyLabel}:{x.IsSelected}:{x.DisplayOrder}");

        return string.Join("|", regionRows)
            + "::"
            + string.Join("|", featureRows)
            + "::"
            + presentation.SelectedRegionId
            + "::"
            + presentation.SelectedConstituencyId
            + "::"
            + presentation.SelectedFeatureId
            + "::"
            + presentation.LatestWinnerPartyLabel;
    }

    private static GameCharacter BuildPlayerCharacter()
    {
        return new GameCharacter(
            "char_player",
            new CharacterName("Test", "Player", null),
            new DateOnly(1980, 1, 1),
            null,
            "nonbinary",
            "unknown",
            "none",
            "unspecified",
            null,
            null,
            0,
            "none",
            new CharacterAttributes(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
            new CharacterHidden(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
            [],
            "ideology_none",
            50,
            0,
            100,
            0,
            [],
            [],
            [],
            [],
            [],
            new Dictionary<string, int>(),
            "none",
            [],
            0,
            true,
            "player_created");
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Westminster.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? "";
        }

        throw new InvalidOperationException("Repository root not found.");
    }
}
