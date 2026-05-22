using Westminster.Core;

namespace Westminster.UK;

public static class UkRegionSeeder
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

    public static List<UkRegion> CreateMvpRegions()
    {
        return new List<UkRegion>
        {
            new("region_north_east", "North East", "england", 1),
            new("region_north_west", "North West", "england", 2),
            new("region_yorkshire_and_humber", "Yorkshire and The Humber", "england", 3),
            new("region_east_midlands", "East Midlands", "england", 4),
            new("region_west_midlands", "West Midlands", "england", 5),
            new("region_east_of_england", "East of England", "england", 6),
            new("region_london", "London", "england", 7),
            new("region_south_east", "South East", "england", 8),
            new("region_south_west", "South West", "england", 9),
            new("region_wales", "Wales", "wales", 10),
            new("region_scotland", "Scotland", "scotland", 11),
            new("region_northern_ireland", "Northern Ireland", "northern_ireland", 12)
        };
    }
}
