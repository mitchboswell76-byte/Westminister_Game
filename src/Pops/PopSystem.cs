using Westminster.Simulation;

namespace Westminster.Pops;

public static class PopSystem
{
    public static void TickMonthly(GameState state)
    {
        if (state.Pops.Count == 0)
        {
            return;
        }

        var metrics = state.MetricsLedger.Snapshot();
        var gdpGrowth = ReadMetric(metrics, "metric.gdp_growth");
        var unemployment = ReadMetric(metrics, "metric.unemployment");
        var inflation = ReadMetric(metrics, "metric.inflation");
        var publicServices = ReadMetric(metrics, "metric.public_services");

        for (var i = 0; i < state.Pops.Count; i++)
        {
            var pop = state.Pops[i];
            var engagement = pop.Engagement;
            var stratum = pop.Stratum;

            if (gdpGrowth > 0)
            {
                if (stratum == "poor" && (i % 10 == 0)) stratum = "middle";
                else if (stratum == "middle" && (i % 25 == 0)) stratum = "wealthy";
                engagement += 0.002 * Math.Min(gdpGrowth, 5d);
            }

            if (unemployment > 0)
            {
                if (stratum == "wealthy" && (i % 20 == 0)) stratum = "middle";
                else if (stratum == "middle" && (i % 8 == 0)) stratum = "poor";
                engagement -= 0.0025 * Math.Min(unemployment, 8d);
            }

            if (inflation > 0 && (stratum == "poor" || stratum == "middle"))
            {
                engagement -= 0.003 * Math.Min(inflation, 8d);
            }

            var ideology = new Dictionary<string, double>(pop.IdeologyVector, StringComparer.Ordinal);
            if (publicServices != 0)
            {
                ideology["social_pa"] = Math.Clamp(ideology.GetValueOrDefault("social_pa") + (publicServices * 0.002), -1d, 1d);
                engagement += 0.0015 * publicServices;
            }

            state.Pops[i] = pop with
            {
                Stratum = stratum,
                Engagement = Math.Clamp(engagement, 0d, 1d),
                IdeologyVector = ideology
            };
        }
    }

    private static double ReadMetric(Dictionary<string, double> metrics, string key) => metrics.TryGetValue(key, out var value) ? value : 0d;
}
