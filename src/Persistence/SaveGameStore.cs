using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Westminster.Core;
using Westminster.Simulation;

namespace Westminster.Persistence;

public sealed class SaveGameStore
{
    private const string ManifestFileName = "manifest.json";
    private const string StateFileName = "state.sqlite";
    private const string SaveFileExtension = ".westminster";

    public void SaveGame(string path, GameState state, ulong seed, SaveSettings settings, bool isAutosave = false)
    {
        if (settings.Ironman && !isAutosave)
        {
            throw new InvalidOperationException("Manual saves are disabled when ironman mode is enabled.");
        }

        var fullPath = EnsureSaveExtension(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");

        var tempDir = Path.Combine(Path.GetTempPath(), $"westminster_save_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var sqlitePath = Path.Combine(tempDir, StateFileName);
            PersistStateToSqlite(sqlitePath, state, seed);

            var manifest = BuildManifest(state, seed, settings);
            var manifestPath = Path.Combine(tempDir, ManifestFileName);
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonSupport.Options), Encoding.UTF8);

            var tempArchivePath = Path.Combine(tempDir, "save.tar.gz");
            using (var file = File.Create(tempArchivePath))
            using (var gzip = new GZipStream(file, CompressionLevel.Optimal))
            using (var tar = new TarWriter(gzip, TarEntryFormat.Pax, leaveOpen: false))
            {
                tar.WriteEntry(manifestPath, ManifestFileName);
                tar.WriteEntry(sqlitePath, StateFileName);
            }

            File.Copy(tempArchivePath, fullPath, overwrite: true);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    public SaveGameStructure LoadMetadata(string path)
    {
        var tempDir = ExtractArchive(path);
        try
        {
            var manifestPath = Path.Combine(tempDir, ManifestFileName);
            var manifestJson = File.ReadAllText(manifestPath, Encoding.UTF8);
            var save = JsonSerializer.Deserialize<SaveGameStructure>(manifestJson, JsonSupport.Options);
            return save ?? throw new InvalidOperationException("Failed to deserialize save manifest.");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    public GameState LoadGame(string path)
    {
        var tempDir = ExtractArchive(path);
        try
        {
            var sqlitePath = Path.Combine(tempDir, StateFileName);
            return LoadStateFromSqlite(sqlitePath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string EnsureSaveExtension(string path) =>
        path.EndsWith(SaveFileExtension, StringComparison.OrdinalIgnoreCase) ? path : path + SaveFileExtension;

    private static SaveGameStructure BuildManifest(GameState state, ulong seed, SaveSettings settings) =>
        new(1, "0.1.0", state.Date, seed, 0UL, state.Player.Id, StateFileName, [], settings);

    private static string ExtractArchive(string path)
    {
        var fullPath = EnsureSaveExtension(path);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Save file not found.", fullPath);

        var tempDir = Path.Combine(Path.GetTempPath(), $"westminster_load_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        using var file = File.OpenRead(fullPath);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        TarFile.ExtractToDirectory(gzip, tempDir, overwriteFiles: true);

        return tempDir;
    }

    private static void PersistStateToSqlite(string sqlitePath, GameState state, ulong seed)
    {
        using var connection = new SqliteConnection($"Data Source={sqlitePath}");
        connection.Open();
        using var transaction = connection.BeginTransaction();

        Execute(connection, transaction, "create table world_state(date text not null, tick_count integer not null, rng_seed integer not null, player_id text not null, monthly_hook_count integer not null, annual_hook_count integer not null, autosave_hook_count integer not null);");
        Execute(connection, transaction, "create table characters(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table cabinet(character_id text primary key);");
        Execute(connection, transaction, "create table constituencies(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table policies(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table schemes(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table events(id text primary key, payload_json text not null);");

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "insert into world_state(date, tick_count, rng_seed, player_id, monthly_hook_count, annual_hook_count, autosave_hook_count) values ($date,$tick,$seed,$player,$monthly,$annual,$autosave);";
            cmd.Parameters.AddWithValue("$date", state.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$tick", (long)state.TickCount);
            cmd.Parameters.AddWithValue("$seed", (long)seed);
            cmd.Parameters.AddWithValue("$player", state.Player.Id);
            cmd.Parameters.AddWithValue("$monthly", state.MonthlyHookCount);
            cmd.Parameters.AddWithValue("$annual", state.AnnualHookCount);
            cmd.Parameters.AddWithValue("$autosave", state.AutosaveHookCount);
            cmd.ExecuteNonQuery();
        }

        InsertJsonRows(connection, transaction, "characters", state.Characters.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "constituencies", state.Constituencies.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "policies", state.Policies.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "schemes", state.SchemesActive.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "events", state.EventQueueToday.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "insert into cabinet(character_id) values ($id);";
            var idParam = cmd.CreateParameter();
            idParam.ParameterName = "$id";
            cmd.Parameters.Add(idParam);
            foreach (var member in state.Cabinet)
            {
                idParam.Value = member.Id;
                cmd.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }

    private static GameState LoadStateFromSqlite(string sqlitePath)
    {
        using var connection = new SqliteConnection($"Data Source={sqlitePath}");
        connection.Open();

        var row = ReadWorldState(connection);
        var characters = ReadJsonRows<Character>(connection, "characters");
        var player = characters.Single(c => c.Id == row.PlayerId);
        var state = new GameState(row.Date, player)
        {
            TickCount = row.TickCount,
            MonthlyHookCount = row.Monthly,
            AnnualHookCount = row.Annual,
            AutosaveHookCount = row.Autosave
        };

        state.Characters.Clear();
        state.Characters.AddRange(characters);
        state.Constituencies.AddRange(ReadJsonRows<Constituency>(connection, "constituencies"));
        state.Policies.AddRange(ReadJsonRows<PolicyLever>(connection, "policies"));
        state.SchemesActive.AddRange(ReadJsonRows<Scheme>(connection, "schemes"));
        state.EventQueueToday.AddRange(ReadJsonRows<GameEvent>(connection, "events"));

        var cabinetIds = ReadCabinetIds(connection);
        foreach (var id in cabinetIds)
        {
            var found = state.Characters.FirstOrDefault(c => c.Id == id);
            if (found is not null) state.Cabinet.Add(found);
        }

        return state;
    }

    private static (DateOnly Date, ulong TickCount, string PlayerId, int Monthly, int Annual, int Autosave) ReadWorldState(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select date, tick_count, player_id, monthly_hook_count, annual_hook_count, autosave_hook_count from world_state limit 1;";
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) throw new InvalidOperationException("No world state found in save.");
        return (
            DateOnly.Parse(reader.GetString(0)),
            (ulong)reader.GetInt64(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetInt32(4),
            reader.GetInt32(5));
    }

    private static List<string> ReadCabinetIds(SqliteConnection connection)
    {
        var ids = new List<string>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select character_id from cabinet;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) ids.Add(reader.GetString(0));
        return ids;
    }

    private static List<T> ReadJsonRows<T>(SqliteConnection connection, string table)
    {
        var rows = new List<T>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"select payload_json from {table};";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var row = JsonSerializer.Deserialize<T>(reader.GetString(0), JsonSupport.Options);
            if (row is null) throw new InvalidOperationException($"Failed to deserialize row from {table}.");
            rows.Add(row);
        }

        return rows;
    }

    private static void InsertJsonRows(SqliteConnection connection, SqliteTransaction transaction, string table, IEnumerable<(string Id, string Json)> rows)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = $"insert into {table}(id, payload_json) values ($id, $json);";
        var idParam = cmd.CreateParameter();
        idParam.ParameterName = "$id";
        cmd.Parameters.Add(idParam);
        var jsonParam = cmd.CreateParameter();
        jsonParam.ParameterName = "$json";
        cmd.Parameters.Add(jsonParam);

        foreach (var row in rows)
        {
            idParam.Value = row.Id;
            jsonParam.Value = row.Json;
            cmd.ExecuteNonQuery();
        }
    }

    private static void Execute(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
