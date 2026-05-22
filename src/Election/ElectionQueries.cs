using Westminster.Core;

namespace Westminster.Election;

public static class ElectionQueries
{
    public static List<Constituency> CreateMvpConstituencies()
    {
        var regions = new[]
        {
            "region_north_east","region_north_west","region_yorkshire_and_humber","region_east_midlands","region_west_midlands","region_east_of_england",
            "region_london","region_south_east","region_south_west","region_wales","region_scotland","region_northern_ireland"
        };

        var list = new List<Constituency>();
        for (var i = 0; i < regions.Length; i++)
        {
            list.Add(new Constituency(
                $"constituency_mvp_{i + 1:00}",
                $"MVP Constituency {i + 1}",
                i == 9 ? "wales" : i == 10 ? "scotland" : i == 11 ? "northern_ireland" : "england",
                regions[i],
                70000 + (i * 1500),
                new ConstituencyResult(14000 + i * 500, 2500 + i * 100, 13000 + i * 450, 4000 + i * 120, 3500 + i * 110, i % 4 == 0 ? 700 : 0, 1800 + i * 90),
                "char_none",
                1000,
                "marginal",
                new ConstituencyDemographics(40, 35000 + i * 800, 30 + (i % 8), 70 - (i % 7), 14 + (i % 5), 50 - (i % 6), 18 + (i % 7), 20 + (i % 6)),
                "mvp_topo",
                $"lad_mvp_{i + 1:00}"
            ));
        }

        return list;
    }
}
