using Westminster.Core;
using Westminster.Simulation;

var player = new Character(
    "char_player", new CharacterName("Test", "Player", null), new DateOnly(1980, 1, 1), null, "nonbinary", "unknown", "none", "unspecified", null, null, 0, "none",
    new CharacterAttributes(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
    new CharacterHidden(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
    [], "ideology_none", 50, 0, 100, 0, [], [], [], [], [], new Dictionary<string, int>(), "none", [], 0, true, "player_created");

var state = new GameState(new DateOnly(2026, 1, 1), player);
var rng = new GameRng(123456);
var systems = new NoOpSystems();

for (var i = 0; i < 400; i++)
{
    SimulationTick.Tick(state, rng, systems);
}

Console.WriteLine($"date={state.Date:yyyy-MM-dd} tick_count={state.TickCount} monthly={state.MonthlyHookCount} annual={state.AnnualHookCount} autosave={state.AutosaveHookCount}");
