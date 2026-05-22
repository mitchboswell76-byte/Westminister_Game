using Westminster.Persistence;
using Westminster.Policy;
using Westminster.Core;
using Westminster.Pops;
using Westminster.Simulation;

static string FindRepositoryRoot()
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

    throw new DirectoryNotFoundException($"Could not find repository root from {AppContext.BaseDirectory}");
}

var repoRoot = FindRepositoryRoot();

var player = new Character(
    "char_player", new CharacterName("Test", "Player", null), new DateOnly(1980, 1, 1), null, "nonbinary", "unknown", "none", "unspecified", null, null, 0, "none",
    new CharacterAttributes(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
    new CharacterHidden(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
    [], "ideology_none", 50, 0, 100, 0, [], [], [], [], [], new Dictionary<string, int>(), "none", [], 0, true, "player_created");

var state = new GameState(new DateOnly(2026, 1, 1), player);
state.Policies.AddRange(ContentLoader.MvpOnly(ContentLoader.LoadPolicyLevers(repoRoot)));
state.Pops.AddRange(PopSeeder.CreateMvpPops());
PolicyEngine.ApplyChange(state, "policy_vat_standard_rate", 0.22);
var rng = new GameRng(123456);
var systems = new NoOpSystems();

for (var i = 0; i < 400; i++)
{
    SimulationTick.Tick(state, rng, systems);
}

Console.WriteLine($"repo_root={repoRoot}");
Console.WriteLine($"date={state.Date:yyyy-MM-dd} tick_count={state.TickCount} monthly={state.MonthlyHookCount} annual={state.AnnualHookCount} autosave={state.AutosaveHookCount} policies_loaded={state.Policies.Count} metrics_keys={state.MetricsLedger.Snapshot().Count}");
Console.WriteLine($"pops_loaded={state.Pops.Count}");
Console.WriteLine($"represented_population_total={PopQueries.TotalRepresentedPopulation(state.Pops)}");
Console.WriteLine($"average_engagement={PopQueries.AverageEngagement(state.Pops):0.0000}");
