namespace Westminster.Core;

public class GameRng
{
    private readonly Random _r;
    public ulong Seed { get; }
    public ulong CallCount { get; private set; }

    public GameRng(ulong seed)
    {
        Seed = seed;
        _r = new Random((int)seed);
    }

    public int Next(int min, int maxInclusive)
    {
        CallCount++;
        return _r.Next(min, maxInclusive + 1);
    }

    public double NextDouble()
    {
        CallCount++;
        return _r.NextDouble();
    }

    public bool RollD100Against(int target) => Next(1, 100) <= target;

    public void RestoreCallCount(ulong callCount)
    {
        // Simpler PRD-compatible restoration: re-seed and advance by NextDouble() calls.
        for (ulong i = 0; i < callCount; i++) _ = NextDouble();
    }
}
