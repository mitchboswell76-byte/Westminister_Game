using System.Text.Json;
using Westminster.Core;
using Westminster.Persistence;
using Westminster.Policy;
using Westminster.Simulation;
using Xunit;

namespace Westminster.Tests;

public class PolicyEngineTests
{
    [Fact] public void PolicyEffect_Calculus_LinearMatchesFormula() => Assert.Equal(4, new LinearPolicyEffect("metric.gdp_growth", 2, 1).Evaluate(3));
    [Fact] public void PolicyEffect_Calculus_PiecewiseMatchesFormula() => Assert.Equal(-2, new PiecewisePolicyEffect("metric.gdp_growth", 1, 3, 5).Evaluate(3));
    [Fact] public void PolicyEffect_Calculus_ExpMatchesFormula() => Assert.Equal(2*Math.Exp(1), new ExpPolicyEffect("metric.gdp_growth", 2, 2).Evaluate(3), 8);
    [Fact] public void PolicyEffect_Calculus_LogMatchesFormula() => Assert.Equal(Math.Log(2), new LogPolicyEffect("metric.gdp_growth", 1, 0).Evaluate(1), 8);
    [Fact] public void PolicyEffect_Calculus_StepMatchesFormula() => Assert.Equal(10, new StepPolicyEffect("metric.gdp_growth", 3, 1, 10).Evaluate(3));

    [Fact]
    public void ContentLoader_LoadsExactly50MvpLevers()
    {
        var root = FindRepositoryRoot();
        var levers = ContentLoader.MvpOnly(ContentLoader.LoadPolicyLevers(root));
        Assert.Equal(50, levers.Count);
    }

    private static string FindRepositoryRoot(){var c=AppContext.BaseDirectory;while(!string.IsNullOrWhiteSpace(c)){if(File.Exists(Path.Combine(c,"Westminster.sln"))) return c; c=Directory.GetParent(c)?.FullName ?? "";} throw new Exception();}
}
