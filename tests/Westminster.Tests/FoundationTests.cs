using System.Text.Json;
using Westminster.Core;
using Westminster.Simulation;

namespace Westminster.Tests;

public class FoundationTests
{
    [Fact]
    public void GameRng_Deterministic_WithSameSeed()
    {
        var a = new GameRng(123456UL);
        var b = new GameRng(123456UL);

        for (var i = 0; i < 50; i++)
        {
            Assert.Equal(a.Next(1, 100), b.Next(1, 100));
        }

        for (var i = 0; i < 25; i++)
        {
            Assert.Equal(a.NextDouble(), b.NextDouble(), 10);
        }
    }

    [Fact]
    public void GameRng_CallCount_IncrementsCorrectly()
    {
        var rng = new GameRng(1UL);
        Assert.Equal(0UL, rng.CallCount);

        _ = rng.Next(1, 10);
        _ = rng.NextDouble();
        _ = rng.RollD100Against(50);

        Assert.Equal(3UL, rng.CallCount);
    }

    [Fact]
    public void GameState_DateAndTick_AdvanceCorrectly()
    {
        var state = BuildState();
        var rng = new GameRng(42UL);
        var systems = new NoOpSystems();

        SimulationTick.Tick(state, rng, systems);

        Assert.Equal(new DateOnly(2026, 1, 2), state.Date);
        Assert.Equal(1UL, state.TickCount);
    }

    [Fact]
    public void MonthlyHook_Fires_At30_60_90()
    {
        var state = BuildState();
        var rng = new GameRng(42UL);
        var systems = new NoOpSystems();

        for (var i = 0; i < 90; i++)
        {
            SimulationTick.Tick(state, rng, systems);
        }

        Assert.Equal(3, state.MonthlyHookCount);
    }

    [Fact]
    public void AnnualHook_Fires_At365()
    {
        var state = BuildState();
        var rng = new GameRng(42UL);
        var systems = new NoOpSystems();

        for (var i = 0; i < 365; i++)
        {
            SimulationTick.Tick(state, rng, systems);
        }

        Assert.Equal(1, state.AnnualHookCount);
    }

    [Fact]
    public void AutosaveHook_Fires_Every7Ticks()
    {
        var state = BuildState();
        var rng = new GameRng(42UL);
        var systems = new NoOpSystems();

        for (var i = 0; i < 28; i++)
        {
            SimulationTick.Tick(state, rng, systems);
        }

        Assert.Equal(4, state.AutosaveHookCount);
    }

    [Fact]
    public void JsonRoundTrip_Character_And_SaveGameStructure()
    {
        var character = BuildCharacter();
        var save = new SaveGameStructure(
            1,
            "0.1.0",
            new DateOnly(2026, 1, 1),
            99UL,
            10UL,
            character.Id,
            "world.db",
            [],
            new SaveSettings(1, true, false)
        );

        var characterJson = JsonSerializer.Serialize(character, JsonSupport.Options);
        var saveJson = JsonSerializer.Serialize(save, JsonSupport.Options);

        var characterRoundTrip = JsonSerializer.Deserialize<Character>(characterJson, JsonSupport.Options);
        var saveRoundTrip = JsonSerializer.Deserialize<SaveGameStructure>(saveJson, JsonSupport.Options);

        Assert.NotNull(characterRoundTrip);
        Assert.NotNull(saveRoundTrip);
        Assert.Equal(character, characterRoundTrip);
        Assert.Equal(save, saveRoundTrip);
    }

    [Fact]
    public void NoDirectSystemRandomUsage_OutsideGameRng()
    {
        var randomReferenceFiles = new List<string>();
        var root = FindRepositoryRoot();
        var srcDir = Path.Combine(root, "src");
        var gameRngPath = Path.GetFullPath(Path.Combine(srcDir, "Core", "GameRng.cs"));

        foreach (var file in Directory.EnumerateFiles(srcDir, "*.cs", SearchOption.AllDirectories))
        {
            var fullPath = Path.GetFullPath(file);
            if (string.Equals(fullPath, gameRngPath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var content = File.ReadAllText(file);
            if (content.Contains("System.Random", StringComparison.Ordinal) || content.Contains("new Random(", StringComparison.Ordinal))
            {
                randomReferenceFiles.Add(Path.GetRelativePath(root, fullPath));
            }
        }

        Assert.True(randomReferenceFiles.Count == 0, $"Found direct Random usage in: {string.Join(", ", randomReferenceFiles)}");
    }

    private static GameState BuildState() => new(new DateOnly(2026, 1, 1), BuildCharacter());

    private static Character BuildCharacter() => new(
        "char_player", new CharacterName("Test", "Player", null), new DateOnly(1980, 1, 1), null, "nonbinary", "unknown", "none", "unspecified", null, null, 0, "none",
        new CharacterAttributes(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
        new CharacterHidden(10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
        [], "ideology_none", 50, 0, 100, 0, [], [], [], [], [], new Dictionary<string, int>(), "none", [], 0, true, "player_created"
    );

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Westminster.sln")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        throw new InvalidOperationException("Could not find repository root containing Westminster.sln");
    }
}
