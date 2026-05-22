using Westminster.Core;
using Westminster.Pops;
using Westminster.Simulation;

namespace Westminster.Election;

public static class ElectionSystem
{
    public const string SystemFptp = "fptp_uk";

    public static readonly string[] PartyIds = ["lab", "con", "ld", "grn", "ref", "ind_corbyn", "other"];

    public static ElectionResult ResolveFptpElection(GameState state, GameRng rng)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(rng);

        var constituencies = state.Constituencies.OrderBy(c => c.Id, StringComparer.Ordinal).ToList();
        var popEngagement = PopQueries.AverageEngagement(state.Pops);
        var econ = PopQueries.AverageIdeologyAxis(state.Pops, "economic_lr");
        var social = PopQueries.AverageIdeologyAxis(state.Pops, "social_pa");
        var env = PopQueries.AverageIdeologyAxis(state.Pops, "environmentalism");
        var unemployment = state.MetricsLedger.Get("metric.unemployment");
        var inflation = state.MetricsLedger.Get("metric.inflation");

        var nationalSwing = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["lab"] = (Math.Max(0, -econ) * 0.05) + (Math.Max(0, social) * 0.03) - (inflation * 0.002),
            ["con"] = (Math.Max(0, econ) * 0.05) + (Math.Max(0, -social) * 0.02) - (unemployment * 0.002),
            ["ld"] = (Math.Abs(econ) * 0.015) + (Math.Max(0, social) * 0.01),
            ["grn"] = (Math.Max(0, env) * 0.06),
            ["ref"] = (Math.Max(0, -social) * 0.03) + (Math.Max(0, -env) * 0.01),
            ["ind_corbyn"] = Math.Max(0, -econ) * 0.005,
            ["other"] = 0.0
        };

        var constituencyResults = new List<ConstituencyElectionResult>(constituencies.Count);
        var seatCounts = PartyIds.ToDictionary(p => p, _ => 0, StringComparer.Ordinal);
        var voteCounts = PartyIds.ToDictionary(p => p, _ => 0, StringComparer.Ordinal);

        foreach (var constituency in constituencies)
        {
            var baselineVotes = BaselineVotes(constituency.Result2024);
            var baseTotalVotes = baselineVotes.Values.Sum();
            var shares = PartyIds.ToDictionary(p => p, p => baseTotalVotes > 0 ? baselineVotes[p] / (double)baseTotalVotes : (1d / PartyIds.Length), StringComparer.Ordinal);

            var localNoise = rng.NextDouble(0d, 1d) - 0.5;
            foreach (var party in PartyIds)
            {
                var incumbentBonus = baselineVotes[party] == baselineVotes.Values.Max() ? 0.0075 : 0d;
                var localDemo = DemographicAlignment(constituency.Demographics, party);
                var engagementEffect = (popEngagement - 0.5) * (party == "grn" || party == "ld" ? 0.02 : 0.01);
                var noise = localNoise * 0.01;
                shares[party] = Math.Max(0.0001, shares[party] + nationalSwing[party] + incumbentBonus + localDemo + engagementEffect + noise);
            }

            Normalize(shares);

            var turnout = Math.Clamp(0.50 + (popEngagement * 0.3) + rng.NextDouble(0d, 0.04), 0.45, 0.85);
            var totalVotes = Math.Max(1, (int)Math.Round(constituency.Electorate * turnout, MidpointRounding.AwayFromZero));
            var votesByParty = PartyIds.ToDictionary(p => p, p => (int)Math.Round(shares[p] * totalVotes, MidpointRounding.AwayFromZero), StringComparer.Ordinal);
            ReconcileVotes(votesByParty, totalVotes);

            var winner = ResolveWinner(votesByParty, rng);
            var ordered = votesByParty.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key, StringComparer.Ordinal).ToList();
            var majority = ordered.Count > 1 ? ordered[0].Value - ordered[1].Value : ordered[0].Value;
            var voteShareByParty = PartyIds.ToDictionary(p => p, p => votesByParty[p] / (double)totalVotes, StringComparer.Ordinal);

            seatCounts[winner] += 1;
            foreach (var party in PartyIds) voteCounts[party] += votesByParty[party];

            constituencyResults.Add(new ConstituencyElectionResult(constituency.Id, winner, votesByParty, voteShareByParty, majority, turnout));
        }

        var aggregateVotes = Math.Max(1, voteCounts.Values.Sum());
        var partyResults = PartyIds
            .Select(p => new ElectionPartyResult(p, voteCounts[p], voteCounts[p] / (double)aggregateVotes, seatCounts[p]))
            .OrderByDescending(p => p.Seats).ThenByDescending(p => p.Votes).ThenBy(p => p.PartyId, StringComparer.Ordinal)
            .ToList();

        var winningParty = partyResults[0].PartyId;
        var resultId = $"election_{state.Date:yyyyMMdd}_{state.ElectionResults.Count + 1:000}";
        return new ElectionResult(resultId, state.Date, SystemFptp, constituencyResults, partyResults, winningParty, constituencies.Count, "mvp_step7");
    }

    private static Dictionary<string, int> BaselineVotes(ConstituencyResult result) => new(StringComparer.Ordinal)
    {
        ["lab"] = Math.Max(0, result.Lab), ["con"] = Math.Max(0, result.Con), ["ld"] = Math.Max(0, result.Ld), ["grn"] = Math.Max(0, result.Grn), ["ref"] = Math.Max(0, result.Ref), ["ind_corbyn"] = Math.Max(0, result.IndCorbyn), ["other"] = Math.Max(0, result.Other)
    };

    private static string ResolveWinner(Dictionary<string, int> votesByParty, GameRng rng)
    {
        var maxVotes = votesByParty.Values.Max();
        var tied = votesByParty.Where(kv => kv.Value == maxVotes).Select(kv => kv.Key).OrderBy(x => x, StringComparer.Ordinal).ToList();
        if (tied.Count == 1) return tied[0];
        if (tied.Count == 2) return rng.NextDouble(0d, 1d) < 0.5 ? tied[0] : tied[1];
        return tied[0];
    }

    private static void Normalize(Dictionary<string, double> shares)
    {
        var total = shares.Values.Sum();
        if (total <= 0) { var eq = 1d / shares.Count; foreach (var key in shares.Keys.ToList()) shares[key] = eq; return; }
        foreach (var key in shares.Keys.ToList()) shares[key] /= total;
    }

    private static void ReconcileVotes(Dictionary<string, int> votesByParty, int totalVotes)
    {
        var delta = totalVotes - votesByParty.Values.Sum();
        if (delta == 0) return;
        var targetParty = votesByParty.OrderByDescending(x => x.Value).ThenBy(x => x.Key, StringComparer.Ordinal).First().Key;
        votesByParty[targetParty] = Math.Max(0, votesByParty[targetParty] + delta);
    }

    private static double DemographicAlignment(ConstituencyDemographics d, string party) => party switch
    {
        "grn" => (d.UniGradPct / 100d - 0.35) * 0.03,
        "lab" => (d.SocialHousingPct / 100d - 0.17) * 0.025,
        "con" => (d.OwnerOccupiedPct / 100d - 0.45) * 0.02,
        "ld" => (d.PrivateRentPct / 100d - 0.20) * 0.015,
        "ref" => (d.WhitePct / 100d - 0.75) * 0.015,
        _ => 0d
    };
}
