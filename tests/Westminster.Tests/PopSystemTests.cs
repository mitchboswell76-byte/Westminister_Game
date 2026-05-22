using Westminster.Core;
using Westminster.Pops;
using Westminster.Persistence;
using Westminster.Simulation;
using GameCharacter = Westminster.Core.Character;
using Xunit;

namespace Westminster.Tests;

public class PopSystemTests
{
    [Fact]
    public void PopSeeder_CreatesExactly1000Pops() => Assert.Equal(1000, PopSeeder.CreateMvpPops().Count);

    [Fact]
    public void PopSeeder_UsesExactly12Regions() => Assert.Equal(12, PopSeeder.CreateMvpPops().Select(p => p.RegionId).Distinct().Count());

    [Fact]
    public void PopSeeder_IdsAreUniqueAndStable()
    {
        var pops = PopSeeder.CreateMvpPops();
        Assert.Equal(1000, pops.Select(p => p.Id).Distinct().Count());
        Assert.Equal("pop_0001", pops[0].Id);
        Assert.Equal("pop_1000", pops[^1].Id);
    }

    [Fact]
    public void PopSeeder_EngagementWithinZeroToOne() => Assert.All(PopSeeder.CreateMvpPops(), p => Assert.InRange(p.Engagement, 0d, 1d));

    [Fact]
    public void PopSeeder_IdeologyVectorContainsRequiredAxes()
    {
        var required = new[] { "economic_lr", "social_pa", "globalism", "environmentalism" };
        Assert.All(PopSeeder.CreateMvpPops(), p => Assert.All(required, axis => Assert.True(p.IdeologyVector.ContainsKey(axis))));
    }

    [Fact]
    public void PopQueries_AggregatesPopulationByRegion()
    {
        var pops = PopSeeder.CreateMvpPops();
        var byRegion = PopQueries.RepresentedPopulationByRegion(pops);
        Assert.Equal(12, byRegion.Count);
        Assert.True(byRegion.Values.All(v => v > 0));
        Assert.Equal(PopQueries.TotalRepresentedPopulation(pops), byRegion.Values.Sum());
    }

    [Fact]
    public void PopSystem_TickMonthly_IsDeterministic()
    {
        var a = BuildState();
        var b = BuildState();
        a.MetricsLedger.Add("metric.gdp_growth", 2.0);
        b.MetricsLedger.Add("metric.gdp_growth", 2.0);

        PopSystem.TickMonthly(a);
        PopSystem.TickMonthly(b);

        Assert.Equal(a.Pops, b.Pops);
    }

    [Fact]
    public void SaveGameStore_LoadGame_RoundTripsPops()
    {
        var state = BuildState();
        var store = new SaveGameStore();
        var path = Path.Combine(Path.GetTempPath(), $"westminster_pop_rt_{Guid.NewGuid():N}.westminster");

        try
        {
            store.SaveGame(path, state, new GameRng(123), new SaveSettings(1, true, false));
            var loaded = store.LoadGame(path);
            Assert.Equal(state.Pops.Count, loaded.State.Pops.Count);
            Assert.Equal(state.Pops[0], loaded.State.Pops[0]);
            Assert.Equal(state.Pops[^1], loaded.State.Pops[^1]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static GameState BuildState()
    {
        var player = new GameCharacter(
            "char_player", new CharacterName("Test", "Player", null), new DateOnly(1980, 1, 1), null, "nonbinary", "unknown", "none", "unspecified", null, null, 0, "none",
            new CharacterAttributes(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
            new CharacterHidden(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
            [], "ideology_none", 50, 0, 100, 0, [], [], [], [], [], new Dictionary<string, int>(), "none", [], 0, true, "player_created");
        var state = new GameState(new DateOnly(2026, 1, 1), player);
        state.Pops.AddRange(PopSeeder.CreateMvpPops());
        return state;
    }
}
