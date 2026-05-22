using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Westminster.Core;
using GameCharacter = Westminster.Core.Character;
using Westminster.Simulation;

namespace Westminster.Persistence;

public sealed record LoadedGame(GameState State, GameRng Rng);

public sealed class SaveGameStore
{
    private const string ManifestFileName = "manifest.json";
    private const string StateFileName = "state.sqlite";
    private const string SaveFileExtension = ".westminster";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public void SaveGame(string path, GameState state, GameRng rng, SaveSettings settings, bool isAutosave = false)
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
            PersistStateToSqlite(sqlitePath, state, rng.CaptureState());

            var manifest = BuildManifest(state, rng, settings);
            var manifestPath = Path.Combine(tempDir, ManifestFileName);
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonSupport.Options), Utf8NoBom);

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
        var fullPath = EnsureSaveExtension(path);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Save file not found.", fullPath);

        using var file = File.OpenRead(fullPath);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new TarReader(gzip);
        TarEntry? entry;
        while ((entry = reader.GetNextEntry()) is not null)
        {
            if (!string.Equals(entry.Name, ManifestFileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            using var stream = entry.DataStream ?? throw new InvalidOperationException("Manifest entry did not contain a stream.");
            using var textReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
            var json = textReader.ReadToEnd();
            if (json.Length > 0 && json[0] == '\uFEFF')
            {
                json = json[1..];
            }

            var save = JsonSerializer.Deserialize<SaveGameStructure>(json, JsonSupport.Options);
            return save ?? throw new InvalidOperationException("Failed to deserialize save manifest.");
        }

        throw new InvalidOperationException("Manifest entry not found in save archive.");
    }

    public LoadedGame LoadGame(string path)
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

    private static SaveGameStructure BuildManifest(GameState state, GameRng rng, SaveSettings settings) =>
        new(1, "0.1.0", state.Date, rng.Seed, rng.CallCount, state.Player.Id, StateFileName, [], settings);

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

    private static void PersistStateToSqlite(string sqlitePath, GameState state, RngState rng)
    {
        using var connection = new SqliteConnection($"Data Source={sqlitePath}");
        connection.Open();
        using var transaction = connection.BeginTransaction();

        Execute(connection, transaction, "create table world_state(date text not null, tick_count integer not null, rng_seed integer not null, rng_call_count integer not null, rng_s0 integer not null, rng_s1 integer not null, rng_s2 integer not null, rng_s3 integer not null, player_id text not null, monthly_hook_count integer not null, annual_hook_count integer not null, autosave_hook_count integer not null, government_type text not null);");
        Execute(connection, transaction, "create table characters(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table cabinet(position integer not null, character_id text not null, primary key(position));");
        Execute(connection, transaction, "create table constituencies(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table policies(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table pops(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table schemes(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table events(id text primary key, payload_json text not null);");
        Execute(connection, transaction, "create table metrics(id text primary key, value real not null);");

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "insert into world_state(date, tick_count, rng_seed, rng_call_count, rng_s0, rng_s1, rng_s2, rng_s3, player_id, monthly_hook_count, annual_hook_count, autosave_hook_count, government_type) values ($date,$tick,$seed,$callCount,$s0,$s1,$s2,$s3,$player,$monthly,$annual,$autosave,$governmentType);";
            cmd.Parameters.AddWithValue("$date", state.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$tick", unchecked((long)state.TickCount));
            cmd.Parameters.AddWithValue("$seed", unchecked((long)rng.Seed));
            cmd.Parameters.AddWithValue("$callCount", unchecked((long)rng.CallCount));
            cmd.Parameters.AddWithValue("$s0", unchecked((long)rng.S0));
            cmd.Parameters.AddWithValue("$s1", unchecked((long)rng.S1));
            cmd.Parameters.AddWithValue("$s2", unchecked((long)rng.S2));
            cmd.Parameters.AddWithValue("$s3", unchecked((long)rng.S3));
            cmd.Parameters.AddWithValue("$player", state.Player.Id);
            cmd.Parameters.AddWithValue("$monthly", state.MonthlyHookCount);
            cmd.Parameters.AddWithValue("$annual", state.AnnualHookCount);
            cmd.Parameters.AddWithValue("$autosave", state.AutosaveHookCount);
            cmd.Parameters.AddWithValue("$governmentType", state.GovernmentType);
            cmd.ExecuteNonQuery();
        }

        InsertJsonRows(connection, transaction, "characters", state.Characters.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "constituencies", state.Constituencies.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "policies", state.Policies.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "pops", state.Pops.OrderBy(x => x.Id, StringComparer.Ordinal).Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "schemes", state.SchemesActive.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertJsonRows(connection, transaction, "events", state.EventQueueToday.Select(x => (x.Id, JsonSerializer.Serialize(x, JsonSupport.Options))));
        InsertMetricRows(connection, transaction, state.MetricsLedger.Snapshot());

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "insert into cabinet(position, character_id) values ($position, $id);";
            var positionParam = cmd.CreateParameter();
            positionParam.ParameterName = "$position";
            cmd.Parameters.Add(positionParam);
            var idParam = cmd.CreateParameter();
            idParam.ParameterName = "$id";
            cmd.Parameters.Add(idParam);
            for (var i = 0; i < state.Cabinet.Count; i++)
            {
                positionParam.Value = i;
                idParam.Value = state.Cabinet[i].Id;
                cmd.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }

    private static LoadedGame LoadStateFromSqlite(string sqlitePath)
    {
        using var connection = new SqliteConnection($"Data Source={sqlitePath}");
        connection.Open();

        var row = ReadWorldState(connection);
        var characters = ReadJsonRows<GameCharacter>(connection, "characters");
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
        state.Pops.AddRange(ReadJsonRows<Pop>(connection, "pops"));
        state.SchemesActive.AddRange(ReadJsonRows<Scheme>(connection, "schemes"));
        state.EventQueueToday.AddRange(ReadJsonRows<GameEvent>(connection, "events"));

        var cabinetIds = ReadCabinetIds(connection);
        foreach (var id in cabinetIds)
        {
            var found = state.Characters.FirstOrDefault(c => c.Id == id);
            if (found is not null) state.Cabinet.Add(found);
        }

        foreach (var m in ReadMetricRows(connection)) state.MetricsLedger.Add(m.Key, m.Value);

        var rng = new GameRng(new RngState(row.RngSeed, row.RngCallCount, row.S0, row.S1, row.S2, row.S3));
        return new LoadedGame(state, rng);
    }

    private static (DateOnly Date, ulong TickCount, ulong RngSeed, ulong RngCallCount, ulong S0, ulong S1, ulong S2, ulong S3, string PlayerId, int Monthly, int Annual, int Autosave, string GovernmentType) ReadWorldState(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select date, tick_count, rng_seed, rng_call_count, rng_s0, rng_s1, rng_s2, rng_s3, player_id, monthly_hook_count, annual_hook_count, autosave_hook_count, government_type from world_state limit 1;";
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) throw new InvalidOperationException("No world state found in save.");
        return (
            DateOnly.Parse(reader.GetString(0)),
            unchecked((ulong)reader.GetInt64(1)),
            unchecked((ulong)reader.GetInt64(2)),
            unchecked((ulong)reader.GetInt64(3)),
            unchecked((ulong)reader.GetInt64(4)),
            unchecked((ulong)reader.GetInt64(5)),
            unchecked((ulong)reader.GetInt64(6)),
            unchecked((ulong)reader.GetInt64(7)),
            reader.GetString(8),
            reader.GetInt32(9),
            reader.GetInt32(10),
            reader.GetInt32(11),
            reader.GetString(12));
    }

    private static List<string> ReadCabinetIds(SqliteConnection connection)
    {
        var ids = new List<string>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select character_id from cabinet order by position;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) ids.Add(reader.GetString(0));
        return ids;
    }

    private static List<T> ReadJsonRows<T>(SqliteConnection connection, string table)
    {
        if (!TableExists(connection, table)) return [];
        var rows = new List<T>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"select payload_json from {table} order by id;";
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

    private static bool TableExists(SqliteConnection connection, string table)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select 1 from sqlite_master where type = 'table' and name = $name limit 1;";
        cmd.Parameters.AddWithValue("$name", table);
        var result = cmd.ExecuteScalar();
        return result is not null;
    }

    private static void Execute(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static void InsertMetricRows(SqliteConnection connection, SqliteTransaction transaction, Dictionary<string, double> rows)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "insert into metrics(id, value) values ($id, $value);";
        var idParam = cmd.CreateParameter(); idParam.ParameterName = "$id"; cmd.Parameters.Add(idParam);
        var valueParam = cmd.CreateParameter(); valueParam.ParameterName = "$value"; cmd.Parameters.Add(valueParam);
        foreach (var row in rows.OrderBy(x => x.Key, StringComparer.Ordinal)) { idParam.Value = row.Key; valueParam.Value = row.Value; cmd.ExecuteNonQuery(); }
    }

    private static Dictionary<string, double> ReadMetricRows(SqliteConnection connection)
    {
        var rows = new Dictionary<string, double>(StringComparer.Ordinal);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select id, value from metrics order by id;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) rows[reader.GetString(0)] = reader.GetDouble(1);
        return rows;
    }
}
