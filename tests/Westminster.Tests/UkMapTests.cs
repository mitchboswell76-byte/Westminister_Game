using Westminster.Core;
using Westminster.Election;
using Westminster.Persistence;
using Westminster.Pops;
using Westminster.Simulation;
using Westminster.UK;
using GameCharacter = Westminster.Core.Character;
using Xunit;

namespace Westminster.Tests;

public class UkMapTests
{
    [Fact]
    public void UkRegionSeeder_CreatesExactly12Regions()
    {
        var regions = UkRegionSeeder.CreateMvpRegions();
        Assert.Equal(12, regions.Count);
    }

    [Fact]
    public void UkRegionSeeder_RegionIdsMatchPopRegions()
    {
        var expectedIds = new HashSet<string>(UkRegionSeeder.RegionIds, StringComparer.Ordinal);
        var actualIds = UkRegionSeeder.CreateMvpRegions().Select(r => r.Id).ToHashSet(StringComparer.Ordinal);
        Assert.Equal(expectedIds, actualIds);
    }

    [Fact]
    public void UkMapSeeder_BindsEveryMvpConstituency()
    {
        var constituencies = ElectionQueries.CreateMvpConstituencies();
        var bindings = UkMapSeeder.CreateMvpBindings(constituencies);
        Assert.True(UkMapQueries.ValidateEveryConstituencyHasRegionBinding(constituencies, bindings));
    }

    [Fact]
    public void UkMapQueries_CountsConstituenciesByRegion()
    {
        var constituencies = ElectionQueries.CreateMvpConstituencies();
        var bindings = UkMapSeeder.CreateMvpBindings(constituencies);
        var counts = UkMapQueries.CountConstituenciesByRegion(bindings);
        Assert.Equal(12, counts.Count);
        Assert.All(counts.Values, v => Assert.Equal(1, v));
    }

    [Fact]
    public void UkMapQueries_ValidatesEveryPopRegionExists()
    {
        var pops = PopSeeder.CreateMvpPops();
        var regions = UkRegionSeeder.CreateMvpRegions();
        Assert.True(UkMapQueries.ValidateEveryPopRegionExistsInUkRegions(pops, regions));
    }

    [Fact]
    public void SaveGameStore_LoadGame_RoundTripsMapData()
    {
        var state = BuildState();
        state.Constituencies.AddRange(ElectionQueries.CreateMvpConstituencies());
        state.UkRegions.AddRange(UkRegionSeeder.CreateMvpRegions());
        state.ConstituencyMapBindings.AddRange(UkMapSeeder.CreateMvpBindings(state.Constituencies));
        state.MapTopologies.AddRange(UkMapSeeder.CreateMvpTopologies());

        var store = new SaveGameStore();
        var rng = new GameRng(987654321UL);
        var path = Path.Combine(Path.GetTempPath(), $"westminster_ukmap_{Guid.NewGuid():N}.westminster");

        try
        {
            store.SaveGame(path, state, rng, new SaveSettings(2, true, false));
            var loaded = store.LoadGame(path).State;
            Assert.Equal(state.UkRegions.Count, loaded.UkRegions.Count);
            Assert.Equal(state.ConstituencyMapBindings.Count, loaded.ConstituencyMapBindings.Count);
            Assert.Equal(state.MapTopologies.Count, loaded.MapTopologies.Count);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static GameState BuildState()
    {
        var player = new GameCharacter(
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

        return new GameState(new DateOnly(2026, 1, 1), player);
    }
}
