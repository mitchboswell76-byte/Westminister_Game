using System.Text.Json;
using Westminster.Core;
using Westminster.Simulation;

namespace Westminster.Persistence;

public sealed class SaveGameStore
{
    public void SaveGame(string path, GameState state, ulong seed, SaveSettings settings)
    {
        var save = BuildSave(path, state, seed, settings);
        var json = JsonSerializer.Serialize(save, JsonSupport.Options);
        File.WriteAllText(path, json);
    }

    public SaveGameStructure LoadMetadata(string path)
    {
        var json = File.ReadAllText(path);
        var save = JsonSerializer.Deserialize<SaveGameStructure>(json, JsonSupport.Options);
        return save ?? throw new InvalidOperationException("Failed to deserialize save metadata.");
    }

    private static SaveGameStructure BuildSave(string path, GameState state, ulong seed, SaveSettings settings)
    {
        var gameVersion = "0.1.0";
        var worldDbPath = Path.ChangeExtension(path, ".db");

        return new SaveGameStructure(
            1,
            gameVersion,
            state.Date,
            seed,
            0UL,
            state.Player.Id,
            worldDbPath,
            [],
            settings
        );
    }
}
