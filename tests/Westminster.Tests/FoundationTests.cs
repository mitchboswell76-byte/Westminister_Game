using Xunit;
using System.Text.Json;
using System.IO.Compression;
using System.Formats.Tar;
using Westminster.Core;
using GameCharacter = Westminster.Core.Character;
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

        var characterRoundTrip = JsonSerializer.Deserialize<GameCharacter>(characterJson, JsonSupport.Options);
        var saveRoundTrip = JsonSerializer.Deserialize<SaveGameStructure>(saveJson, JsonSupport.Options);

        Assert.NotNull(characterRoundTrip);
        Assert.NotNull(saveRoundTrip);

        Assert.Equal(character.Id, characterRoundTrip!.Id);
        Assert.Equal(character.Name.First, characterRoundTrip.Name.First);
        Assert.Equal(character.Name.Last, characterRoundTrip.Name.Last);
        Assert.Equal(character.BirthDate, characterRoundTrip.BirthDate);
        Assert.Equal(character.CareerRank, characterRoundTrip.CareerRank);
        Assert.Equal(character.CurrentPosition, characterRoundTrip.CurrentPosition);
        Assert.Equal(character.Attributes.Charisma, characterRoundTrip.Attributes.Charisma);
        Assert.Equal(character.Hidden.Loyalty, characterRoundTrip.Hidden.Loyalty);
        Assert.Equal(character.IdeologyId, characterRoundTrip.IdeologyId);
        Assert.Equal(character.IdeologyPurity, characterRoundTrip.IdeologyPurity);
        Assert.Equal(character.Stress, characterRoundTrip.Stress);
        Assert.Equal(character.Energy, characterRoundTrip.Energy);
        Assert.Equal(character.IsPlayer, characterRoundTrip.IsPlayer);
        Assert.Equal(character.SpawnSource, characterRoundTrip.SpawnSource);

        Assert.Equal(character.Traits, characterRoundTrip.Traits);
        Assert.Equal(character.Relationships.Count, characterRoundTrip.Relationships.Count);
        Assert.Equal(character.HooksHeld, characterRoundTrip.HooksHeld);
        Assert.Equal(character.HooksAgainstMe, characterRoundTrip.HooksAgainstMe);
        Assert.Equal(character.Secrets, characterRoundTrip.Secrets);
        Assert.Equal(character.PerksUnlocked, characterRoundTrip.PerksUnlocked);
        Assert.Equal(character.SchemesActive, characterRoundTrip.SchemesActive);
        Assert.Equal(character.PerkXp.Count, characterRoundTrip.PerkXp.Count);
        foreach (var kvp in character.PerkXp)
        {
            Assert.True(characterRoundTrip.PerkXp.TryGetValue(kvp.Key, out var value));
            Assert.Equal(kvp.Value, value);
        }

        Assert.Equal(save.SaveVersion, saveRoundTrip!.SaveVersion);
        Assert.Equal(save.GameVersion, saveRoundTrip.GameVersion);
        Assert.Equal(save.GameDate, saveRoundTrip.GameDate);
        Assert.Equal(save.RngSeed, saveRoundTrip.RngSeed);
        Assert.Equal(save.RngCallCount, saveRoundTrip.RngCallCount);
        Assert.Equal(save.PlayerCharacterId, saveRoundTrip.PlayerCharacterId);
        Assert.Equal(save.WorldStateDb, saveRoundTrip.WorldStateDb);
        Assert.Equal(save.Settings, saveRoundTrip.Settings);
        Assert.Equal(save.CharactersDirty, saveRoundTrip.CharactersDirty);
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

    private static GameCharacter BuildCharacter() => new(
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

    [Fact]
    public void CharacterFactory_CreatesPlayableBackbencherWithDefaults()
    {
        var character = Westminster.Character.CharacterFactory.CreatePlayer(
            "char_alex_smith",
            "Alex",
            "Smith",
            new DateOnly(1990, 5, 5),
            "constituency_test",
            "party_labour"
        );

        Assert.Equal("char_alex_smith", character.Id);
        Assert.Equal("Alex", character.Name.First);
        Assert.Equal("backbencher", character.CurrentPosition);
        Assert.Equal("party_labour", character.PartyId);
        Assert.True(character.IsPlayer);
    }

    [Fact]
    public void SaveGameStore_CanPersistAndReadSaveMetadata()
    {
        var state = BuildState();
        var store = new Westminster.Persistence.SaveGameStore();
        var savePath = Path.Combine(Path.GetTempPath(), $"westminster_test_{Guid.NewGuid():N}.westminster");

        try
        {
            store.SaveGame(savePath, state, 12345UL, new SaveSettings(2, true, false));
            var loaded = store.LoadMetadata(savePath);

            Assert.Equal(1, loaded.SaveVersion);
            Assert.Equal(state.Date, loaded.GameDate);
            Assert.Equal("char_player", loaded.PlayerCharacterId);
            Assert.Equal(12345UL, loaded.RngSeed);
            Assert.Equal("state.sqlite", loaded.WorldStateDb);
        }
        finally
        {
            if (File.Exists(savePath)) File.Delete(savePath);
        }
    }

    [Fact]
    public void SaveGameStore_SaveArchive_ContainsManifestAndSqlite()
    {
        var state = BuildState();
        var store = new Westminster.Persistence.SaveGameStore();
        var savePath = Path.Combine(Path.GetTempPath(), $"westminster_test_{Guid.NewGuid():N}.westminster");

        try
        {
            store.SaveGame(savePath, state, 12345UL, new SaveSettings(2, true, false));

            using var file = File.OpenRead(savePath);
            using var gzip = new GZipStream(file, CompressionMode.Decompress);
            using var reader = new TarReader(gzip);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            TarEntry? entry;
            while ((entry = reader.GetNextEntry()) is not null)
            {
                names.Add(entry.Name);
            }

            Assert.Contains("manifest.json", names);
            Assert.Contains("state.sqlite", names);
        }
        finally
        {
            if (File.Exists(savePath)) File.Delete(savePath);
        }
    }

    [Fact]
    public void SaveGameStore_Ironman_BlocksManualButAllowsAutosave()
    {
        var state = BuildState();
        var store = new Westminster.Persistence.SaveGameStore();
        var savePath = Path.Combine(Path.GetTempPath(), $"westminster_test_{Guid.NewGuid():N}.westminster");

        Assert.Throws<InvalidOperationException>(() =>
            store.SaveGame(savePath, state, 777UL, new SaveSettings(1, true, true), isAutosave: false));

        store.SaveGame(savePath, state, 777UL, new SaveSettings(1, true, true), isAutosave: true);
        Assert.True(File.Exists(savePath));

        if (File.Exists(savePath)) File.Delete(savePath);
    }

    [Fact]
    public void SaveGameStore_LoadGame_RoundTripsCoreState()
    {
        var state = BuildState();
        state.TickCount = 100;
        state.MonthlyHookCount = 3;
        state.Policies.Add(new PolicyLever("policy_vat","VAT","tax","indirect","slider",0,30,1,20,20,"%","int",[],[],1,"MVP"));
        var cabinetMember = state.Player with { Id = "char_cabinet_1", Name = new CharacterName("Casey", "Minister", null), IsPlayer = false };
        state.Characters.Add(cabinetMember);
        state.Cabinet.Add(cabinetMember);

        var store = new Westminster.Persistence.SaveGameStore();
        var savePath = Path.Combine(Path.GetTempPath(), $"westminster_test_{Guid.NewGuid():N}.westminster");
        try
        {
            store.SaveGame(savePath, state, 999UL, new SaveSettings(1, true, false));
            var loaded = store.LoadGame(savePath);

            Assert.Equal(state.Date, loaded.Date);
            Assert.Equal(100UL, loaded.TickCount);
            Assert.Equal(2, loaded.Characters.Count);
            Assert.Single(loaded.Cabinet);
            Assert.Single(loaded.Policies);
        }
        finally
        {
            if (File.Exists(savePath)) File.Delete(savePath);
        }
    }

}
