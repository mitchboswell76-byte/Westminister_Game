using Westminster.Core;

namespace Westminster.UK;

public static class UkTopologyValidator
{
    public static bool ValidateFeatures(List<MapFeature> features, out string message)
    {
        foreach (var feature in features)
        {
            if (string.IsNullOrWhiteSpace(feature.Id))
            {
                message = "Feature is missing Id.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(feature.TopojsonObjectId))
            {
                message = $"Feature {feature.Id} is missing TopojsonObjectId.";
                return false;
            }
        }

        var duplicateFeatureId = features
            .GroupBy(x => x.Id, StringComparer.Ordinal)
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicateFeatureId is not null)
        {
            message = $"Duplicate feature id {duplicateFeatureId.Key}.";
            return false;
        }

        var duplicateTopo = features
            .GroupBy(x => x.TopojsonObjectId, StringComparer.Ordinal)
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicateTopo is not null)
        {
            message = $"Duplicate topojson object id {duplicateTopo.Key}.";
            return false;
        }

        var validRegionIds = UkRegionSeeder.CreateMvpRegions().Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
        var invalidRegion = features.FirstOrDefault(x => !validRegionIds.Contains(x.RegionId));
        if (invalidRegion is not null)
        {
            message = $"Feature {invalidRegion.Id} has unknown region {invalidRegion.RegionId}.";
            return false;
        }

        message = "ok";
        return true;
    }

    public static bool ValidateBindingsAgainstFeatures(List<ConstituencyMapBinding> bindings, List<MapFeature> features, out string message)
    {
        var featureTopoIds = features.Select(x => x.TopojsonObjectId).ToHashSet(StringComparer.Ordinal);
        foreach (var binding in bindings)
        {
            if (!featureTopoIds.Contains(binding.TopojsonObjectId))
            {
                message = $"Binding for constituency {binding.ConstituencyId} is missing feature for topo id {binding.TopojsonObjectId}.";
                return false;
            }
        }

        message = "ok";
        return true;
    }
}
