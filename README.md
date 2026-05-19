# WESTMINSTER (Foundation: Phase 0 / Phase 1)

This repository contains the initial Godot 4.3 + C# foundation from `Westminster_PRD.md`.

## Run from GitHub Codespaces (Chromebook-friendly)

This repo includes a dev container at `.devcontainer/devcontainer.json` so you can run everything from a browser-based Codespaces terminal with no local Linux setup.

### 1) Open in Codespaces
1. In GitHub, click **Code**.
2. Open the **Codespaces** tab.
3. Create a new Codespace on your branch.
4. Wait for the container to finish setup (`dotnet restore` runs automatically once).

### 2) Terminal-only workflow (recommended)
Run these commands from the repository root:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project tools/Westminster.SmokeRunner
python scripts/check_no_direct_random.py
python scripts/smoke_checks.py
```

Notes:
- `dotnet test` currently validates build/test discovery and will succeed even when no dedicated test project exists yet.
- The smoke runner advances 400 simulation days and prints deterministic counters.

## Optional: headless Godot check in Codespaces

You do **not** need the desktop Godot editor for this PR. If you want a Godot runtime check in terminal only, use headless mode:

```bash
godot4 --headless --path . --quit
```

If `godot4` is not found in your Codespace image, install a headless build inside the Codespace and rerun the same command.

## Local prerequisites (outside Codespaces)
- .NET 8 SDK
- (Optional) Godot 4.3 runtime/editor for opening `project.godot`
