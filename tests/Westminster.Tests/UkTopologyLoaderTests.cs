using Westminster.Core;
using Westminster.Election;
using Westminster.UK;
using Xunit;

namespace Westminster.Tests;

public class UkTopologyLoaderTests
{
    [Fact]
    public void UkTopologyLoader_LoadsFixtureMetadata()
    {
        var root = FindRepositoryRoot();
        var metadata = UkTopologyLoader.LoadTopologyMetadata(root);

        Assert.Single(metadata);
        Assert.Equal("topology_mvp_constituencies", metadata[0].Id);
    }

    [Fact]
    public void UkTopologyLoader_LoadsFixtureFeatures()
    {
        var root = FindRepositoryRoot();
        var collection = UkTopologyLoader.LoadConstituencyFeatures(root);

        Assert.Equal("topology_mvp_constituencies", collection.TopologyId);
        Assert.True(collection.Features.Count >= 12);
    }

    [Fact]
    public void UkTopologyValidator_RejectsDuplicateFeatureIds()
    {
        var features = new List<MapFeature>
        {
            new("feature_1", "A", "region_london", "constituency_mvp_01", "topo_1"),
            new("feature_1", "B", "region_london", "constituency_mvp_02", "topo_2")
        };

        var isValid = UkTopologyValidator.ValidateFeatures(features, out _);
        Assert.False(isValid);
    }

    [Fact]
    public void UkTopologyValidator_ValidatesBindingsAgainstFeatures()
    {
        var constituencies = ElectionQueries.CreateMvpConstituencies();
        var bindings = UkMapSeeder.CreateMvpBindings(constituencies);
        var collection = UkTopologyLoader.LoadConstituencyFeatures(FindRepositoryRoot());

        var isValid = UkTopologyValidator.ValidateBindingsAgainstFeatures(bindings, collection.Features, out _);
        Assert.True(isValid);
    }

    [Fact]
    public void UkTopologyValidator_RejectsMissingBindingFeature()
    {
        var bindings = new List<ConstituencyMapBinding>
        {
            new("constituency_mvp_01", "region_london", "missing_topo", "lad_1")
        };

        var features = new List<MapFeature>
        {
            new("feature_1", "A", "region_london", "constituency_mvp_01", "topo_1")
        };

        var isValid = UkTopologyValidator.ValidateBindingsAgainstFeatures(bindings, features, out _);
        Assert.False(isValid);
    }

    [Fact]
    public void UkTopologyLoader_UsesRepositoryRootNotBinPath()
    {
        var repositoryRoot = FindRepositoryRoot();
        var simulatedBinPath = Path.Combine(repositoryRoot, "tools", "Westminster.SmokeRunner", "bin", "Debug", "net8.0");
        Directory.CreateDirectory(simulatedBinPath);

        var root = UkTopologyLoader.FindRepositoryRootFromBaseDirectory(simulatedBinPath);
        Assert.Equal(repositoryRoot, root);
    }

    private static string FindRepositoryRoot()
    {
        return UkTopologyLoader.FindRepositoryRootFromBaseDirectory(AppContext.BaseDirectory);
    }
}
