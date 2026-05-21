using Westminster.Core;
using Westminster.Policy;
using GameCharacter = Westminster.Core.Character;

namespace Westminster.Simulation;

public interface IInputProcessor { void Process(GameState s); }
public interface ISchemeSystem { void TickScheme(Scheme scheme, GameState s, GameRng rng); }
public interface IEventSystem { void ResolveEvent(GameEvent ev, GameState s, GameRng rng); }
public interface ICharacterSystem { void TickCharacter(GameCharacter character, GameState s, GameRng rng); }
public interface IMonthlySystem { void TickMonthly(GameState s, GameRng rng); }
public interface IAnnualSystem { void TickAnnual(GameState s, GameRng rng); }
public interface IAutosaveSystem { void AutoSave(GameState s); }
public interface IUiDirtyMarker { void MarkUiDirty(GameState s); }

public sealed class NoOpSystems : IInputProcessor, ISchemeSystem, IEventSystem, ICharacterSystem, IMonthlySystem, IAnnualSystem, IAutosaveSystem, IUiDirtyMarker
{
    public void Process(GameState s) { }
    public void TickScheme(Scheme scheme, GameState s, GameRng rng) { }
    public void ResolveEvent(GameEvent ev, GameState s, GameRng rng) { }
    public void TickCharacter(GameCharacter character, GameState s, GameRng rng) { }
    public void TickMonthly(GameState s, GameRng rng) { PolicyEngine.TickMonthly(s); s.MonthlyHookCount++; }
    public void TickAnnual(GameState s, GameRng rng) => s.AnnualHookCount++;
    public void AutoSave(GameState s) => s.AutosaveHookCount++;
    public void MarkUiDirty(GameState s) { }
}
