# WESTMINSTER Development History

## 1. Project Summary

WESTMINSTER is a political simulation project being built from `Westminster_PRD.md`, with a deterministic simulation-first foundation and incremental implementation following the PRD’s fixed AI build order.

- **Target engine:** Godot 4.3 + C# (.NET 8).
- **Current development phase:** Phase 1 MVP implementation in progress, with Steps 1–7 complete.
- **Current PRD build-order step:** Step 8 (UK map/topology integration) is **in progress** (8A/8B/8C delivered, rendering refinement pending).

---

## 2. PRD Build Order Tracker

| Step | PRD task | Status | Repo evidence | Notes | Next action |
|---|---|---|---|---|---|
| 1 | Project skeleton | Done | `project.godot`, `src/Westminster.Game.csproj`, modular folders under `src/` | Base Godot/C# structure exists and builds in solution. | Keep layout stable while adding features. |
| 2 | §4 data schemas as C# records with JSON serialisation | Done | `src/Core/Models.cs`, JSON options in `JsonSupport` | Core schema records are present and JSON round-trip is tested. | Validate fields against PRD continuously when expanding usage. |
| 3 | §5 core simulation tick loop | Done | `src/Simulation/SimulationTick.cs`, `src/Simulation/GameState.cs`, foundation tests | Daily tick, periodic hooks (monthly/annual/autosave), deterministic RNG wiring exist. | Replace no-op systems with real systems gradually, without breaking determinism. |
| 4 | Character creation flow + SQLite save/load | Done | `src/Character/CharacterFactory.cs`, `src/Persistence/SaveGameStore.cs`, persistence integration tests | Character creation request model and hybrid `.westminster` save format (manifest + SQLite) are now implemented with transactional writes and load path; broader gameplay entities are still placeholders. | Save/load round-trip determinism now passes PRD §20.4 integration coverage with persisted full RNG state and ordered reads. |
| 5 | Policy engine with 50 MVP policies | Done | `src/Policy/PolicyEngine.cs`, `src/Policy/Effects.cs`, `content/policies/*.json`, `src/Persistence/ContentLoader.cs` | Policy engine, content loader, metrics ledger, and 50 MVP levers now integrated with save/load and smoke checks. | Proceed to Step 6 pop model (1,000 pops/12 regions). |
| 6 | Pop model | Done | `src/Pops/PopSeeder.cs`, `src/Pops/PopSystem.cs`, `src/Pops/PopQueries.cs`, `src/Simulation/GameState.cs` | Deterministic 1,000-pop model across 12 UK regions, monthly drift, aggregates, persistence, and tests added. | Proceed to Step 7 election system (FPTP simulator). |
| 7 | Election system | Done | `src/Election/ElectionSystem.cs`, `src/Election/ElectionQueries.cs`, `tests/Westminster.Tests/ElectionSystemTests.cs` | Deterministic MVP FPTP simulator, aggregation, and persistence integration implemented. | Proceed to Step 8 UK map. |
| 8 | UK map | In progress (Steps 8A/8B/8C foundation + view model scaffold) | `src/UK/UkRegionSeeder.cs`, `src/UK/UkMapSeeder.cs`, `src/UK/UkMapQueries.cs` | Deterministic UK region/map binding fixture foundation added; full topology ingestion/visual map pending. | Step 8D: map rendering refinement (player-facing map presentation) or Step 9 cabinet system after map scaffold follow-up. |
| 9 | Cabinet system | Not started | `src/Faction/Placeholder.cs` (related systems still placeholder) | No ministerial post logic or May 2026 seed data in gameplay systems yet. | Implement cabinet posts/appointments and seed data. |
| 10 | MVP schemes | Not started | `src/Narrative/Placeholder.cs` | Scheme framework beyond basic tick list placeholders not implemented. | Implement 5 PRD MVP schemes. |
| 11 | Polish/playtest/ship MVP | Not started | N/A | Pre-MVP stabilization and playtest phases not reached. | Reach functional MVP scope before polish/ship work. |

---

## 3. Update History

## 2026-05-21 — PR #7 — Repo cleanup: ignore generated artifacts

- **Summary of changes:** Added a repository `.gitignore` to keep generated build outputs, IDE files, save artifacts, generated SQLite/DB files, and temporary extraction folders out of version control.
- **Files/areas changed:** `.gitignore`, `docs/DEVELOPMENT_HISTORY.md`.
- **Tests/checks run:** Solution restore/build/test, smoke runner, and Python guard/smoke scripts.
- **Result:** Cleaner git status during local/devcontainer and CI workflows with no gameplay or persistence behavior changes.
- **Known limitations:** Ignore patterns are intentionally broad for generated artifacts; tracked files remain unaffected.
- **Next recommended step (current):** Continue Step 4 persistence coverage expansion while keeping generated artifacts untracked.

## 2026-05-19 — PR #1 — Phase 0/1 foundation: project skeleton, schemas, tick loop, RNG, and smoke checks

- **Summary of changes:** Established Godot/C# project skeleton, core schema records, deterministic RNG wrapper, initial simulation tick loop, and smoke/test scaffolding.
- **Files/areas changed:** `project.godot`, `src/Core/*`, `src/Simulation/*`, `src/*/Placeholder.cs`, `tests/Westminster.Tests/*`, `scripts/*`.
- **Tests/checks run:** .NET tests and smoke checks were introduced with deterministic assertions.
- **Result:** Foundation became runnable and testable.
- **Known limitations:** No gameplay systems beyond foundation; no policy/pop/election/UI gameplay loops.
- **Next recommended step (at that time):** Make the repository easily runnable in Codespaces and add a terminal smoke runner.

## 2026-05-19 — PR #2 — Add Codespaces terminal workflow and smoke runner project

- **Summary of changes:** Added Codespaces/devcontainer-friendly workflow and dedicated smoke runner executable.
- **Files/areas changed:** `.devcontainer/*`, `tools/Westminster.SmokeRunner/*`, README run instructions.
- **Tests/checks run:** Build/run workflow documented for restore/build/test/smoke.
- **Result:** Browser-based terminal workflow enabled for Chromebook-friendly development.
- **Known limitations:** Workflow required later cleanup/stabilization for structure consistency.
- **Next recommended step (at that time):** Stabilize project layout, solution wiring, and tests.

## 2026-05-19 — PR #3 — Stabilize solution layout, runner wiring, and foundation tests

- **Summary of changes:** Cleaned and stabilized solution/project wiring, test project placement, and runner/test reliability.
- **Files/areas changed:** `Westminster.sln`, `tests/Westminster.Tests/*`, `tools/Westminster.SmokeRunner/*`, related references.
- **Tests/checks run:** Solution-level restore/build/test and smoke execution flow.
- **Result:** More consistent project structure and improved baseline verification.
- **Known limitations:** Still foundation-only; feature systems largely placeholders.
- **Next recommended step (at that time):** Begin PRD Step 4 (character creation + persistence foundation).

## 2026-05-19 — PR #4 — Implement Phase 1 character creation and save metadata store

- **Summary of changes:** Added initial character creation factory and save metadata store (JSON save metadata scaffold), plus tests.
- **Files/areas changed:** `src/Character/CharacterFactory.cs`, `src/Persistence/SaveGameStore.cs`, `tests/Westminster.Tests/FoundationTests.cs`.
- **Tests/checks run:** Foundation tests extended for character factory and save metadata persistence.
- **Result:** Step 4 started with scaffolding in place.
- **Known limitations:** SQLite world-state save/load per PRD is not complete; currently metadata-oriented persistence.
- **Next recommended step (current):** Stabilize foundation tooling/layout and then complete Step 4 with true SQLite save/load.

## 2026-05-19 — Repository setup / PRD baseline import

- **Summary of changes:** Initial repository and PRD content uploaded (pre-PR foundation commits).
- **Files/areas changed:** Initial docs and repository bootstrap content.
- **Tests/checks run:** Not clearly traceable in current history.
- **Result:** Repository initialized and ready for implementation PRs.
- **Known limitations:** Early bootstrap commits are coarse-grained.
- **Next recommended step (historical):** Start fixed PRD build order from skeleton onward.

## 2026-05-21 — PR #5 — Hybrid save archive persistence, CI integration, and Step 4 hardening

- **2026-05-21 follow-up:** PR #6 required a build-fix for `Character` namespace/type collision (CS0118) by using explicit model aliases in affected files; persistence behavior unchanged.
- **Summary of changes:** Replaced metadata-only save logic with hybrid `.westminster` archive persistence using `manifest.json` + `state.sqlite`; added transactional SQLite writes/reads for mutable state scaffolding; added richer character-creation request flow; and introduced GitHub Actions CI for restore/build/test/smoke checks.
- **Files/areas changed:** `src/Persistence/SaveGameStore.cs`, `src/Simulation/GameState.cs`, `src/Character/CharacterFactory.cs`, `src/Westminster.Game.csproj`, `tests/Westminster.Tests/FoundationTests.cs`, `.github/workflows/ci.yml`, `README.md`.
- **Tests/checks run:** Solution restore/build/test, smoke runner, Python random guard, Python smoke checks.
- **Result:** Foundation workflow and persistence architecture are now aligned with PRD §17.4 hybrid format and deterministic transaction-oriented storage expectations.
- **Known limitations:** Most simulation modules remain placeholders, so persisted tables currently store only implemented mutable structures.
- **Next recommended step (current):** Keep Step 4 in progress while expanding persisted content as modules become implemented, then proceed to Step 5 policy engine once Step 4 acceptance coverage is complete.

## 2026-05-21 — PR #8 — Complete Step 4 RNG determinism and save/load round-trip

Status: Completed
PRD step: Step 4 (Character creation + SQLite save/load)
Summary: Replaced `System.Random` usage in `GameRng` with deterministic xoshiro256** + splitmix64 seeding, persisted full RNG internal state in SQLite world state, made save/load return both `GameState` and `GameRng`, enforced deterministic SELECT ordering (including cabinet position), added PRD §20.4 save/load determinism integration coverage plus RNG capture/restore sequence test, and widened direct-random guard script to include `tools/` and `tests/`.
Files changed: `src/Core/GameRng.cs`, `src/Persistence/SaveGameStore.cs`, `tests/Westminster.Tests/FoundationTests.cs`, `scripts/check_no_direct_random.py`, `docs/DEVELOPMENT_HISTORY.md`.
Tests/checks run: `dotnet restore Westminster.sln`, `dotnet build Westminster.sln`, `dotnet test Westminster.sln --no-build`, `dotnet run --project tools/Westminster.SmokeRunner/Westminster.SmokeRunner.csproj`, `python scripts/check_no_direct_random.py`, `python scripts/smoke_checks.py`.
Result: Save→load→continue now preserves deterministic trajectory with byte-identical canonical hash outcomes versus never-saved control, and Step 4 acceptance coverage is green including PRD §20.4.
Known limitations: Policy engine and broader gameplay modules remain pending per fixed build order (Step 5 onward).
Next recommended action: Implement Step 5 policy engine with the 50 MVP policy levers.

## 2026-05-21 — PR #9 — Step 5 policy engine + 50 MVP policy levers

Status: Completed
PRD step: Step 5 (Policy engine + 50 MVP levers)
Summary: Implemented PRD-aligned PolicyLever schema, pure policy effect calculus, monthly policy evaluation engine, metrics ledger persistence, policy content loader, and 50 MVP policy lever JSON files; integrated monthly tick ordering, smoke runner policy load/change path, and smoke checks for policy content constraints.
Files changed: `src/Core/Models.cs`, `src/Policy/*`, `src/Persistence/ContentLoader.cs`, `src/Persistence/SaveGameStore.cs`, `src/Simulation/*`, `content/policies/*.json`, `tools/Westminster.SmokeRunner/Program.cs`, `scripts/smoke_checks.py`, `tests/Westminster.Tests/PolicyEngineTests.cs`, `docs/DEVELOPMENT_HISTORY.md`.
Tests/checks run: restore/build/test/smoke runner/random guard/smoke checks.
Result: Step 5 core implementation complete with deterministic monthly policy metrics accumulation and content loaded from disk.
Known limitations: Effect magnitudes are placeholder values pending playtest calibration; advanced policy enactment and downstream systems remain Phase 2+.
Next recommended action: Implement Step 6 pop model (1,000 pops across 12 regions).

---

## 4. Current Repo State

Current repository state includes:

- **Godot stub and root project files:** `project.godot`, `scenes/`, `scripts/`.
- **C# project structure:** `Westminster.sln` with `src/Westminster.Game.csproj`, `tests/Westminster.Tests`, and `tools/Westminster.SmokeRunner`.
- **Module folders:** `Character`, `Core`, `Election`, `Faction`, `Narrative`, `Persistence`, `Policy`, `Pops`, `Simulation`, `UI`, `UK`, `World`.
- **Core models:** Extensive PRD-aligned record types in `src/Core/Models.cs`.
- **GameRng:** Deterministic RNG abstraction implemented in `src/Core/GameRng.cs`.
- **GameState:** Tick/date/player/hook counters and core collections in `src/Simulation/GameState.cs`.
- **Simulation tick loop:** Daily tick progression and periodic hooks in `src/Simulation/SimulationTick.cs`.
- **Smoke scripts:** `scripts/check_no_direct_random.py` and `scripts/smoke_checks.py`.
- **README run commands:** restore/build/test/smoke plus Python smoke checks and optional headless Godot command.
- **Path/layout notes:** Current layout is materially improved versus initial state; however, persistence implementation and full PRD Step 4 scope remain incomplete.

---

## 5. Current Known Issues / Technical Debt

- Hybrid archive + SQLite persistence is in place, but table coverage is currently limited to implemented modules (many gameplay systems still placeholders).
- Policy engine (Step 5) is implemented, but effect balancing is still placeholder-level and requires iterative tuning.
- Pop model (Step 6) is implemented at MVP scope; advanced demographic dynamics remain intentionally out of scope for now.
- MVP election simulator (Step 7) implemented; future expansion can add richer polling/media and non-FPTP systems.
- No implemented cabinet gameplay system yet (Step 9 not started).
- No implemented MVP schemes yet (Step 10 not started).
- UI remains stub-level; no real player-facing gameplay loop yet.
- MVP automated coverage now includes policy (Step 5), pop (Step 6), and election (Step 7) systems with deterministic and persistence-focused tests.
- Remaining coverage is still MVP-level; deeper end-to-end integration scenarios should be expanded in later phases.

---

## 6. Next Recommended PR

### Primary next PR
**Proceed to Step 8 UK map (ONS BGC topology integration, PRD §16.2/§18.5).**

Rationale: Steps 1–7 are now complete, and the fixed PRD build order now moves to Step 8.

---

## 7. Rules for Future AI Assistants

For every future PR (Codex, Claude, or human-assisted):

1. Always read `docs/DEVELOPMENT_HISTORY.md` before starting work.
2. Always update `docs/DEVELOPMENT_HISTORY.md` before finishing a PR.
3. Always add a new entry under **Update History**.
4. Always update the **PRD Build Order Tracker** if project status changes.
5. Always record tests/checks run.
6. Never mark a PRD step as **Done** unless it is implemented and tested.
7. Do not skip the PRD build order from `Westminster_PRD.md`.
8. Do not implement P2+ features before MVP foundation is complete.

---

## 8. Entry Template

```markdown
## YYYY-MM-DD — PR #X — Title

Status:
PRD step:
Summary:
Files changed:
Tests/checks run:
Result:
Known limitations:
Next recommended action:
```


## 2026-05-22 — PR #10 — Implement Step 6 MVP pop model

- **Summary of changes:** Implemented PRD Step 6 with deterministic 1,000-pop seeding across 12 UK regions, monthly pop drift reacting to policy metrics ledger, pop aggregate query helpers, and pop save/load persistence in SQLite archive saves.
- **Files/areas changed:** `src/Core/Models.cs`, `src/Simulation/GameState.cs`, `src/Simulation/Systems.cs`, `src/Pops/*`, `src/Persistence/SaveGameStore.cs`, `tools/Westminster.SmokeRunner/Program.cs`, `scripts/smoke_checks.py`, `tests/Westminster.Tests/PopSystemTests.cs`, `docs/DEVELOPMENT_HISTORY.md`.
- **Tests/checks run:** Restore/build/test, smoke runner, no-direct-random check, smoke checks.
- **Result:** Step 6 is complete and deterministic with persistence and verification coverage.
- **Known limitations:** Pop drift remains intentionally simple MVP logic without elections/factions simulation depth.
- **Next recommended step (current):** Implement Step 7 election system (FPTP simulator).


## 2026-05-22 — PR #11 — Implement Step 7 FPTP election system

- **Summary of changes:** Replaced election placeholder with deterministic MVP FPTP simulation using constituency baselines plus pop/policy-driven swing, added MVP constituency fallback seeding, persisted election result history, wired smoke runner election output, and added election test coverage including determinism, tie-breaks, and save/load round-trip.
- **Files/areas changed:** `src/Election/*`, `src/Core/Models.cs`, `src/Simulation/GameState.cs`, `src/Persistence/SaveGameStore.cs`, `tools/Westminster.SmokeRunner/Program.cs`, `tests/Westminster.Tests/ElectionSystemTests.cs`, `scripts/smoke_checks.py`, `docs/DEVELOPMENT_HISTORY.md`.
- **Tests/checks run:** Restore/build/test, smoke runner, no-direct-random guard, smoke checks.
- **Result:** Step 7 election system is now implemented at MVP scope and integrated with persistence and CI smoke flows.
- **Known limitations:** Uses small deterministic MVP constituency fixture data when full UK constituency/map import is not yet available (Step 8).
- **Next recommended step (current):** Implement Step 8 UK map and topology integration.


## 2026-05-22 — PR #13 — Implement Step 8A UK map data foundation

- **Summary of changes:** Implemented Step 8A data foundation with UK region records, constituency-region map bindings, topology metadata fixtures, UK map query helpers, smoke runner counters, save/load persistence coverage, and dedicated UK map tests.
- **Files/areas changed:** `src/Core/Models.cs`, `src/Simulation/GameState.cs`, `src/UK/*`, `src/Persistence/SaveGameStore.cs`, `tools/Westminster.SmokeRunner/Program.cs`, `tests/Westminster.Tests/UkMapTests.cs`, `scripts/smoke_checks.py`, `docs/DEVELOPMENT_HISTORY.md`.
- **Tests/checks run:** Restore/build/test, smoke runner, no-direct-random guard, smoke checks.
- **Result:** Step 8 is now partially complete (8A foundation); deterministic region + binding substrate is present without implementing full visual map/topology import yet.
- **Known limitations:** Uses MVP fixture topology metadata and placeholder object/LAD IDs; full ONS/BGC topology loading and visual rendering remain for Step 8B+.
- **Next recommended step (current):** Step 8B topology loader or visual map scaffold.


## 2026-05-22 — PR #14 — Implement Step 8B UK topology loader

- **Summary of changes:** Added a fixture-based UK topology asset contract with repository-root content loading, deterministic ordered deserialization, topology/feature validation helpers, smoke runner topology counters, and dedicated Step 8B loader/validator tests.
- **Files/areas changed:** `content/map/uk/*`, `src/Core/Models.cs`, `src/UK/UkTopologyLoader.cs`, `src/UK/UkTopologyValidator.cs`, `tools/Westminster.SmokeRunner/Program.cs`, `tests/Westminster.Tests/UkTopologyLoaderTests.cs`, `scripts/smoke_checks.py`, `docs/DEVELOPMENT_HISTORY.md`.
- **Tests/checks run:** Python guard and smoke checks; .NET checks deferred to CI when local dotnet unavailable.
- **Result:** Step 8 remains in progress; Step 8B topology loader + asset contract foundation is complete without visual map UI.
- **Known limitations:** Topology assets are MVP fixtures only; full ONS/BGC ingestion and rendered map flow remain pending.
- **Next recommended step (current):** Step 8C visual map scaffold or topology import expansion.


## 2026-05-23 — PR #15 — Implement Step 8C UK map visual scaffold

- **Summary of changes:** Added a deterministic, testable UK map view-model layer (region/feature display models + builder), wired election winner projection and selection flags, emitted map view counters from smoke runner, and added dedicated Step 8C tests and smoke script checks.
- **Files/areas changed:** `src/UI/Map/*`, `tools/Westminster.SmokeRunner/Program.cs`, `tests/Westminster.Tests/UkMapViewModelTests.cs`, `scripts/smoke_checks.py`, `docs/DEVELOPMENT_HISTORY.md`.
- **Tests/checks run:** restore/build/test, smoke runner, no-direct-random check, smoke checks, and CI `ci / build-test-smoke`.
- **Result:** Step 8 remains in progress; Step 8C visual scaffold / map view model is complete with deterministic projection coverage, while full rendered topology/map visuals are still pending.
- **Known limitations:** Godot UI rendering remains intentionally minimal; no full GIS rendering or large real ONS/BGC topology ingestion yet.
- **Next recommended step (current):** Step 8D map rendering refinement (surface the view model in player-facing Godot UI), or proceed to Step 9 cabinet system once map rendering scope is accepted.
