namespace Westminster.Core;

public record RngState(ulong Seed, ulong CallCount, ulong S0, ulong S1, ulong S2, ulong S3);

public class GameRng
{
    public ulong Seed { get; }
    public ulong CallCount { get; private set; }

    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;

    public GameRng(ulong seed)
    {
        Seed = seed;
        var smState = seed;
        _s0 = NextSplitMix64(ref smState);
        _s1 = NextSplitMix64(ref smState);
        _s2 = NextSplitMix64(ref smState);
        _s3 = NextSplitMix64(ref smState);
    }

    public GameRng(RngState state)
    {
        Seed = state.Seed;
        CallCount = state.CallCount;
        _s0 = state.S0;
        _s1 = state.S1;
        _s2 = state.S2;
        _s3 = state.S3;
    }

    public RngState CaptureState() => new(Seed, CallCount, _s0, _s1, _s2, _s3);

    public int Next(int min, int maxInclusive)
    {
        if (maxInclusive < min) throw new ArgumentOutOfRangeException(nameof(maxInclusive), "maxInclusive must be >= min.");

        CallCount++;

        var range = (ulong)((long)maxInclusive - min + 1L);
        if (range == 0UL) return (int)NextUInt64(); // full int range wrap case

        var threshold = (0UL - range) % range;

        while (true)
        {
            var r = NextUInt64();
            var product = ((UInt128)r) * range;
            var low = (ulong)product;

            if (low >= threshold)
            {
                var high = (ulong)(product >> 64);
                return min + (int)high;
            }
        }
    }

    public double NextDouble()
    {
        CallCount++;
        return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
    }

    public bool RollD100Against(int target) => Next(1, 100) <= target;

    private ulong NextUInt64()
    {
        var result = RotateLeft(_s1 * 5UL, 7) * 9UL;

        var t = _s1 << 17;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;
        _s3 = RotateLeft(_s3, 45);

        return result;
    }

    private static ulong RotateLeft(ulong x, int k) => (x << k) | (x >> (64 - k));

    private static ulong NextSplitMix64(ref ulong state)
    {
        state += 0x9E3779B97F4A7C15UL;
        var z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
