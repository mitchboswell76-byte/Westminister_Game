# WESTMINSTER (Foundation: Phase 0 / Phase 1)

This repository contains the initial Godot 4.3 + C# foundation from `Westminster_PRD.md`.

## Run from GitHub Codespaces (Chromebook-friendly)

This repo includes a dev container at `.devcontainer/devcontainer.json` so you can run everything from a browser-based Codespaces terminal with no local Linux setup.

### 1) Open in Codespaces
1. In GitHub, click **Code**.
2. Open the **Codespaces** tab.
3. Create a new Codespace on your branch.
4. Wait for the container to finish setup.

### 2) Terminal-only workflow (recommended)
Run these commands from the repository root:

```bash
dotnet restore Westminster.sln
dotnet build Westminster.sln
dotnet test Westminster.sln
dotnet run --project tools/Westminster.SmokeRunner/Westminster.SmokeRunner.csproj
python scripts/check_no_direct_random.py
python scripts/smoke_checks.py
```

The smoke runner advances 400 simulation days and prints deterministic counters.

## Optional: headless Godot check in Codespaces

You do **not** need the desktop Godot editor for this foundation. If you want a runtime check in terminal-only mode:

```bash
godot4 --headless --path . --quit
```

If `godot4` is not available in your Codespace image, skip this optional check.

## Local prerequisites (outside Codespaces)
- .NET 8 SDK
- (Optional) Godot 4.3 runtime/editor for opening `project.godot`
