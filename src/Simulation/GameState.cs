using Westminster.Core;
using Westminster.Policy;
using GameCharacter = Westminster.Core.Character;

namespace Westminster.Simulation;

public class GameState
{
    public DateOnly Date { get; set; }
    public ulong TickCount { get; set; }
    public GameCharacter Player { get; set; }
    public List<GameCharacter> Cabinet { get; } = [];
    public List<GameCharacter> Characters { get; } = [];
    public List<Constituency> Constituencies { get; } = [];
    public List<PolicyLever> Policies { get; } = [];
    public List<Scheme> SchemesActive { get; } = [];
    public List<GameEvent> EventQueueToday { get; } = [];

    public int MonthlyHookCount { get; set; }
    public int AnnualHookCount { get; set; }
    public int AutosaveHookCount { get; set; }
    public string GovernmentType { get; set; } = "parliamentary_constitutional_monarchy";
    public MetricsLedger MetricsLedger { get; } = new();

    public GameState(DateOnly startDate, GameCharacter player)
    {
        Date = startDate;
        Player = player;
        Characters.Add(player);
    }
}
