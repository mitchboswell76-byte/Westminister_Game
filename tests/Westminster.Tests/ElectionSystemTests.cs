using Westminster.Core;
using Westminster.Election;
using Westminster.Persistence;
using Westminster.Pops;
using Westminster.Simulation;
using GameCharacter = Westminster.Core.Character;
using Xunit;

namespace Westminster.Tests;

public class ElectionSystemTests
{
    [Fact]
    public void ElectionSystem_ProducesOneResultPerConstituency()
    {
        var (state, rng) = BuildElectionState();
        var result = ElectionSystem.ResolveFptpElection(state, rng);
        Assert.Equal(state.Constituencies.Count, result.ConstituencyResults.Count);
    }

    [Fact]
    public void ElectionSystem_AggregatesSeatsToTotalConstituencies()
    {
        var (state, rng) = BuildElectionState();
        var result = ElectionSystem.ResolveFptpElection(state, rng);
        Assert.Equal(state.Constituencies.Count, result.PartyResults.Sum(x => x.Seats));
    }

    [Fact]
    public void ElectionSystem_IsDeterministicForSameSeedAndState()
    {
        var (stateA, rngA) = BuildElectionState();
        var (stateB, rngB) = BuildElectionState();
        var a = ElectionSystem.ResolveFptpElection(stateA, rngA);
        var b = ElectionSystem.ResolveFptpElection(stateB, rngB);
        Assert.Equal(Canonicalize(a), Canonicalize(b));
    }

    [Fact]
    public void ElectionSystem_WinnerHasMostSeats()
    {
        var (state, rng) = BuildElectionState();
        var result = ElectionSystem.ResolveFptpElection(state, rng);
        var maxSeats = result.PartyResults.Max(x => x.Seats);
        Assert.Equal(maxSeats, result.PartyResults.Single(x => x.PartyId == result.WinningPartyId).Seats);
    }

    [Fact]
    public void ElectionSystem_DoesNotUseDirectRngType()
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "Election", "ElectionSystem.cs"));
        Assert.DoesNotContain("System." + "Random", content, StringComparison.Ordinal);
        Assert.DoesNotContain("new " + "Random(", content, StringComparison.Ordinal);
    }

    [Fact]
    public void SaveGameStore_LoadGame_RoundTripsElectionResults()
    {
        var (state, rng) = BuildElectionState();
        state.ElectionResults.Add(ElectionSystem.ResolveFptpElection(state, rng));
        var store = new SaveGameStore();
        var path = Path.Combine(Path.GetTempPath(), $"westminster_election_{Guid.NewGuid():N}.westminster");
        try
        {
            store.SaveGame(path, state, rng, new SaveSettings(2, true, false));
            var loaded = store.LoadGame(path).State;
            Assert.Single(loaded.ElectionResults);
            Assert.Equal(Canonicalize(state.ElectionResults[0]), Canonicalize(loaded.ElectionResults[0]));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void ElectionSystem_HandlesMissingOrZeroVoteBaselineSafely()
    {
        var (state, rng) = BuildElectionState();
        state.Constituencies.Clear();
        state.Constituencies.Add(new Constituency("c0","Zero","england","region_london",50000,new ConstituencyResult(0,0,0,0,0,0,0),"char",0,"safe",new ConstituencyDemographics(40,30000,40,60,20,40,20,25),"t","l"));
        var result = ElectionSystem.ResolveFptpElection(state, rng);
        Assert.Single(result.ConstituencyResults);
        Assert.True(result.ConstituencyResults[0].VotesByParty.Values.Sum() > 0);
    }

    [Fact]
    public void ElectionSystem_TieBreakIsDeterministic()
    {
        var state = BuildState();
        state.Pops.AddRange(PopSeeder.CreateMvpPops());
        state.Constituencies.Add(new Constituency("c_tie","Tie","england","region_london",10000,new ConstituencyResult(100,0,100,0,0,0,0),"char",0,"marginal",new ConstituencyDemographics(40,30000,40,60,20,40,20,25),"t","l"));
        var a = ElectionSystem.ResolveFptpElection(state, new GameRng(42));
        var b = ElectionSystem.ResolveFptpElection(state, new GameRng(42));
        Assert.Equal(a.ConstituencyResults[0].WinnerPartyId, b.ConstituencyResults[0].WinnerPartyId);
    }

    private static (GameState, GameRng) BuildElectionState()
    {
        var state = BuildState();
        state.Pops.AddRange(PopSeeder.CreateMvpPops());
        state.Constituencies.AddRange(ElectionQueries.CreateMvpConstituencies());
        return (state, new GameRng(123456UL));
    }

    private static GameState BuildState() => new(new DateOnly(2026,1,1), new GameCharacter("char_player", new CharacterName("Test","Player",null), new DateOnly(1980,1,1), null, "nonbinary", "unknown", "none", "unspecified", null, null, 0, "none", new CharacterAttributes(10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10), new CharacterHidden(10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10), [], "ideology_none", 50, 0, 100, 0, [], [], [], [], [], new Dictionary<string, int>(), "none", [], 0, true, "player_created"));

    private static string Canonicalize(ElectionResult r) => string.Join("|", r.ConstituencyResults.OrderBy(x=>x.ConstituencyId).Select(x => $"{x.ConstituencyId}:{x.WinnerPartyId}:{string.Join(',', x.VotesByParty.OrderBy(k=>k.Key).Select(k=>$\"{k.Key}={k.Value}\"))}"));

    private static string FindRepositoryRoot(){ var cur=AppContext.BaseDirectory; while(!string.IsNullOrWhiteSpace(cur)){ if(File.Exists(Path.Combine(cur,"Westminster.sln"))) return cur; cur=Directory.GetParent(cur)?.FullName ?? "";} throw new InvalidOperationException(); }
}
