# WESTMINSTER Development History

## 1. Project Summary

WESTMINSTER is a political simulation project being built from `Westminster_PRD.md`, with a deterministic simulation-first foundation and incremental implementation following the PRD’s fixed AI build order.

- **Target engine:** Godot 4.3 + C# (.NET 8).
- **Current development phase:** Foundation complete, early Phase 1 transition (character creation/save metadata scaffolding started).
- **Current PRD build-order step:** Step 4 (Character creation flow + SQLite save/load) is **in progress**.

---

## 2. PRD Build Order Tracker

| Step | PRD task | Status | Repo evidence | Notes | Next action |
|---|---|---|---|---|---|
| 1 | Project skeleton | Done | `project.godot`, `src/Westminster.Game.csproj`, modular folders under `src/` | Base Godot/C# structure exists and builds in solution. | Keep layout stable while adding features. |
| 2 | §4 data schemas as C# records with JSON serialisation | Done | `src/Core/Models.cs`, JSON options in `JsonSupport` | Core schema records are present and JSON round-trip is tested. | Validate fields against PRD continuously when expanding usage. |
| 3 | §5 core simulation tick loop | Done | `src/Simulation/SimulationTick.cs`, `src/Simulation/GameState.cs`, foundation tests | Daily tick, periodic hooks (monthly/annual/autosave), deterministic RNG wiring exist. | Replace no-op systems with real systems gradually, without breaking determinism. |
| 4 | Character creation flow + SQLite save/load | In progress | `src/Character/CharacterFactory.cs`, `src/Persistence/SaveGameStore.cs`, persistence integration tests | Character creation request model and hybrid `.westminster` save format (manifest + SQLite) are now implemented with transactional writes and load path; broader gameplay entities are still placeholders. | Expand persisted tables as gameplay modules become concrete and add deterministic regression fixtures. |
| 5 | Policy engine with 50 MVP policies | Not started | `src/Policy/Placeholder.cs` | No active policy execution engine yet. | Implement policy lever model execution and MVP 50 levers. |
| 6 | Pop model | Not started | `src/Pops/Placeholder.cs` | Pop simulation not implemented. | Build 1,000-pop/12-region MVP model. |
| 7 | Election system | Not started | `src/Election/Placeholder.cs` | FPTP simulator not implemented. | Add election cycle + constituency result simulation. |
| 8 | UK map | Not started | `src/UK/Placeholder.cs`, `src/World/Placeholder.cs` | No ONS BGC map/topology integration yet. | Import map assets and build region/constituency binding. |
| 9 | Cabinet system | Not started | `src/Faction/Placeholder.cs` (related systems still placeholder) | No ministerial post logic or May 2026 seed data in gameplay systems yet. | Implement cabinet posts/appointments and seed data. |
| 10 | MVP schemes | Not started | `src/Narrative/Placeholder.cs` | Scheme framework beyond basic tick list placeholders not implemented. | Implement 5 PRD MVP schemes. |
| 11 | Polish/playtest/ship MVP | Not started | N/A | Pre-MVP stabilization and playtest phases not reached. | Reach functional MVP scope before polish/ship work. |

---

## 3. Update History

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
- No implemented policy engine yet (Step 5 not started).
- No implemented pop model yet (Step 6 not started).
- No implemented election simulator yet (Step 7 not started).
- No implemented cabinet gameplay system yet (Step 9 not started).
- No implemented MVP schemes yet (Step 10 not started).
- UI remains stub-level; no real player-facing gameplay loop yet.
- Test coverage is foundation-centric; feature-specific integration tests do not yet exist because those features are not yet implemented.

---

## 6. Next Recommended PR

### Primary next PR
**Expand Step 4 persistence coverage and add deterministic regression fixtures for additional mutable entities.**

Rationale: keep the base toolchain and developer workflow robust before deeper feature additions, ensuring future PRs can be validated quickly and consistently.

### Then proceed to the next PRD feature
**Proceed to Step 5 policy engine after Step 4 acceptance coverage is complete.**

Rationale: this is the next fixed build-order item (Step 4) and is only partially complete today.

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
