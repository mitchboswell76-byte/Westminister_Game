using Westminster.Core;
using Westminster.Election;
using Westminster.Simulation;
using Westminster.UI.Map;
using Westminster.UK;
using GameCharacter = Westminster.Core.Character;
using Xunit;

namespace Westminster.Tests;

public class UkMapViewModelTests
{
    [Fact]
    public void UkMapViewModelBuilder_BuildsOneRegionPerSeededRegion()
    {
        var state = BuildMapState();
        var collection = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());

        var viewModel = UkMapViewModelBuilder.Build(state, collection);

        Assert.Equal(state.UkRegions.Count, viewModel.Regions.Count);
    }

    [Fact]
    public void UkMapViewModelBuilder_BuildsOneFeaturePerTopologyFeature()
    {
        var state = BuildMapState();
        var collection = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());

        var viewModel = UkMapViewModelBuilder.Build(state, collection);

        Assert.Equal(collection.Features.Count, viewModel.Features.Count);
    }

    [Fact]
    public void UkMapViewModelBuilder_MapsElectionWinnersToFeatures()
    {
        var state = BuildMapState();
        state.ElectionResults.Add(ElectionSystem.ResolveFptpElection(state, new GameRng(55UL)));
        var collection = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());

        var viewModel = UkMapViewModelBuilder.Build(state, collection);

        Assert.Contains(viewModel.Features, x => !string.IsNullOrWhiteSpace(x.WinnerPartyId));
    }

    [Fact]
    public void UkMapViewModelBuilder_SelectionMarksSelectedRegion()
    {
        var state = BuildMapState();
        var collection = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());
        var selectedRegionId = state.UkRegions[0].Id;

        var viewModel = UkMapViewModelBuilder.Build(state, collection, selectedRegionId, null);

        Assert.Contains(viewModel.Regions, x => x.RegionId == selectedRegionId && x.IsSelected);
    }

    [Fact]
    public void UkMapViewModelBuilder_SelectionMarksSelectedConstituency()
    {
        var state = BuildMapState();
        var collection = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());
        var selectedConstituencyId = collection.Features[0].ConstituencyId;

        var viewModel = UkMapViewModelBuilder.Build(state, collection, null, selectedConstituencyId);

        Assert.Contains(viewModel.Features, x => x.ConstituencyId == selectedConstituencyId && x.IsSelected);
    }

    [Fact]
    public void UkMapViewModelBuilder_IsDeterministic()
    {
        var stateA = BuildMapState();
        var stateB = BuildMapState();
        var collectionA = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());
        var collectionB = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());

        var a = UkMapViewModelBuilder.Build(stateA, collectionA, "region_london", "const_london_central");
        var b = UkMapViewModelBuilder.Build(stateB, collectionB, "region_london", "const_london_central");

        Assert.Equal(Canonicalize(a), Canonicalize(b));
    }

    [Fact]
    public void UkMapViewModelBuilder_ReportsInvalidWhenBindingMissing()
    {
        var state = BuildMapState();
        var collection = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());
        state.ConstituencyMapBindings[0] = state.ConstituencyMapBindings[0] with { TopojsonObjectId = "missing_topo_id" };

        var viewModel = UkMapViewModelBuilder.Build(state, collection);

        Assert.False(viewModel.IsValid);
        Assert.Contains(viewModel.ValidationMessages, message => message.Contains("missing feature", StringComparison.OrdinalIgnoreCase));
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

    private static string Canonicalize(UkMapViewModel model)
    {
        var regionRows = model.Regions
            .OrderBy(x => x.RegionId, StringComparer.Ordinal)
            .Select(x => $"{x.RegionId}:{x.ConstituencyCount}:{x.IsSelected}:{x.DisplayOrder}");

        var featureRows = model.Features
            .OrderBy(x => x.FeatureId, StringComparer.Ordinal)
            .Select(x => $"{x.FeatureId}:{x.ConstituencyId}:{x.RegionId}:{x.WinnerPartyId}:{x.IsSelected}:{x.DisplayOrder}");

        var validationRows = model.ValidationMessages.OrderBy(x => x, StringComparer.Ordinal);

        return string.Join("|", regionRows)
            + "::"
            + string.Join("|", featureRows)
            + "::"
            + string.Join("|", validationRows)
            + "::"
            + model.TopologyId
            + "::"
            + model.IsValid;
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
