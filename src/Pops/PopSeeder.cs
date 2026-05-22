using Westminster.Core;

namespace Westminster.Pops;

public static class PopSeeder
{
    public static readonly string[] RegionIds =
    [
        "region_north_east",
        "region_north_west",
        "region_yorkshire_and_humber",
        "region_east_midlands",
        "region_west_midlands",
        "region_east_of_england",
        "region_london",
        "region_south_east",
        "region_south_west",
        "region_wales",
        "region_scotland",
        "region_northern_ireland"
    ];

    private static readonly string[] Strata = ["poor", "middle", "wealthy"];
    private static readonly string[] Professions = ["services", "industry", "public_sector", "agriculture", "retail", "technology"];
    private static readonly string[] AgeCohorts = ["18-24", "25-44", "45-64", "65+"];
    private static readonly string[] Educations = ["school", "vocational", "degree", "postgraduate"];
    private static readonly string[] Ethnicities = ["white_british", "asian_british", "black_british", "mixed", "other"];
    private static readonly string[] Religions = ["none", "christian", "muslim", "hindu", "sikh", "other"];

    public static List<Pop> CreateMvpPops()
    {
        var pops = new List<Pop>(1000);
        for (var i = 0; i < 1000; i++)
        {
            var idx = i + 1;
            var region = RegionIds[i % RegionIds.Length];
            var stratum = Strata[i % Strata.Length];

            var ideology = new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["economic_lr"] = -0.8 + ((i % 21) / 20.0) * 1.6,
                ["social_pa"] = -0.9 + (((i / 2) % 19) / 18.0) * 1.8,
                ["globalism"] = -0.7 + (((i / 3) % 15) / 14.0) * 1.4,
                ["environmentalism"] = -0.6 + (((i / 5) % 13) / 12.0) * 1.2,
            };

            var pop = new Pop(
                $"pop_{idx:0000}",
                region,
                500 + (i % 2500),
                stratum,
                Professions[(i + (i / 7)) % Professions.Length],
                ideology,
                Ethnicities[(i / 5) % Ethnicities.Length],
                Religions[(i / 11) % Religions.Length],
                AgeCohorts[(i / 13) % AgeCohorts.Length],
                Educations[(i / 17) % Educations.Length],
                Math.Clamp(0.35 + ((i % 53) / 100.0), 0d, 1d)
            );

            pops.Add(pop);
        }

        return pops;
    }
}
