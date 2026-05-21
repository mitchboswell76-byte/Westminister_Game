using System.Text.Json.Serialization;

namespace Westminster.Policy;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "function")]
[JsonDerivedType(typeof(LinearPolicyEffect), typeDiscriminator: "linear")]
[JsonDerivedType(typeof(PiecewisePolicyEffect), typeDiscriminator: "piecewise")]
[JsonDerivedType(typeof(ExpPolicyEffect), typeDiscriminator: "exp")]
[JsonDerivedType(typeof(LogPolicyEffect), typeDiscriminator: "log")]
[JsonDerivedType(typeof(StepPolicyEffect), typeDiscriminator: "step")]
public abstract record PolicyEffect(string Target)
{
    public abstract double Evaluate(double currentValue);
}

public sealed record LinearPolicyEffect(string Target, double Slope, double Intercept) : PolicyEffect(Target)
{
    public override double Evaluate(double currentValue) => Slope * (currentValue - Intercept);
}

public sealed record PiecewisePolicyEffect(string Target, double BelowSlope, double AboveSlope, double Knee) : PolicyEffect(Target)
{
    public override double Evaluate(double currentValue) =>
        currentValue < Knee ? BelowSlope * (currentValue - Knee) : AboveSlope * (currentValue - Knee);
}

public sealed record ExpPolicyEffect(string Target, double K, double X0) : PolicyEffect(Target)
{
    public override double Evaluate(double currentValue) => K * Math.Exp(currentValue - X0);
}

public sealed record LogPolicyEffect(string Target, double K, double X0) : PolicyEffect(Target)
{
    public override double Evaluate(double currentValue) => K * Math.Log(1 + (currentValue - X0));
}

public sealed record StepPolicyEffect(string Target, double Threshold, double ValueBelow, double ValueAbove) : PolicyEffect(Target)
{
    public override double Evaluate(double currentValue) => currentValue < Threshold ? ValueBelow : ValueAbove;
}
