# WESTMINSTER (Foundation: Phase 0 / Phase 1)

This repository contains the initial Godot 4.3 + C# foundation from `Westminster_PRD.md`.

## Prerequisites
- .NET 8 SDK
- (Optional) Godot 4.3 Mono editor/runtime for opening `project.godot`

## Build
```bash
dotnet build
```

## Run debug/console smoke runner (400 days)
```bash
dotnet run --project src/Westminster.DebugRunner/Westminster.DebugRunner.csproj
```

## Run tests
```bash
dotnet test
```

## Additional smoke check: direct `System.Random` guard
```bash
python scripts/check_no_direct_random.py
```
