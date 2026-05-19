using Westminster.Core;

namespace Westminster.Simulation;

public class GameState
{
    public DateOnly Date { get; set; }
    public ulong TickCount { get; set; }
    public Character Player { get; set; }
    public List<Character> Cabinet { get; } = [];
    public List<Scheme> SchemesActive { get; } = [];
    public List<GameEvent> EventQueueToday { get; } = [];

    public int MonthlyHookCount { get; set; }
    public int AnnualHookCount { get; set; }
    public int AutosaveHookCount { get; set; }

    public GameState(DateOnly startDate, Character player)
    {
        Date = startDate;
        Player = player;
    }
}
