using System.Text.Json;
using Westminster.Core;

namespace Westminster.UK;

public static class UkTopologyLoader
{
    public static List<MapTopologyMetadata> LoadTopologyMetadataFromRepository()
    {
        var root = FindRepositoryRootFromBaseDirectory(AppContext.BaseDirectory);
        return LoadTopologyMetadata(root);
    }

    public static MapFeatureCollection LoadConstituencyFeaturesFromRepository()
    {
        var root = FindRepositoryRootFromBaseDirectory(AppContext.BaseDirectory);
        return LoadConstituencyFeatures(root);
    }

    public static List<MapTopologyMetadata> LoadTopologyMetadata(string repositoryRoot)
    {
        var metadataPath = Path.Combine(repositoryRoot, "content", "map", "uk", "mvp_topology_metadata.json");
        if (!File.Exists(metadataPath))
        {
            throw new FileNotFoundException($"Missing topology metadata fixture at {metadataPath}.");
        }

        var json = File.ReadAllText(metadataPath);
        var data = JsonSerializer.Deserialize<List<MapTopologyMetadata>>(json, JsonSupport.Options);
        if (data is null)
        {
            throw new InvalidOperationException($"Failed to deserialize topology metadata from {metadataPath}.");
        }

        return data.OrderBy(x => x.Id, StringComparer.Ordinal).ToList();
    }

    public static MapFeatureCollection LoadConstituencyFeatures(string repositoryRoot)
    {
        var featurePath = Path.Combine(repositoryRoot, "content", "map", "uk", "mvp_constituency_features.json");
        if (!File.Exists(featurePath))
        {
            throw new FileNotFoundException($"Missing constituency feature fixture at {featurePath}.");
        }

        var json = File.ReadAllText(featurePath);
        var data = JsonSerializer.Deserialize<MapFeatureCollection>(json, JsonSupport.Options);
        if (data is null)
        {
            throw new InvalidOperationException($"Failed to deserialize constituency features from {featurePath}.");
        }

        var ordered = data.Features.OrderBy(x => x.Id, StringComparer.Ordinal).ToList();
        return new MapFeatureCollection(data.Id, data.TopologyId, ordered);
    }

    public static string FindRepositoryRootFromBaseDirectory(string baseDirectory)
    {
        var current = baseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Westminster.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? "";
        }

        throw new DirectoryNotFoundException($"Could not find repository root from {baseDirectory}");
    }
}
