using Westminster.Core;

namespace Westminster.Simulation;

public static class SimulationTick
{
    public static void Tick(GameState s, GameRng rng, NoOpSystems systems)
    {
        systems.Process(s);
        s.Date = s.Date.AddDays(1);
        s.TickCount++;
        foreach (var scheme in s.SchemesActive) systems.TickScheme(scheme, s, rng);
        foreach (var ev in s.EventQueueToday) systems.ResolveEvent(ev, s, rng);
        systems.TickCharacter(s.Player, s, rng);
        foreach (var minister in s.Cabinet) systems.TickCharacter(minister, s, rng);
        if (s.TickCount % 30 == 0) systems.TickMonthly(s, rng);
        if (s.TickCount % 365 == 0) systems.TickAnnual(s, rng);
        if (s.TickCount % 7 == 0) systems.AutoSave(s);
        systems.MarkUiDirty(s);
    }
}
