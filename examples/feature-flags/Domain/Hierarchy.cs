namespace FeatureFlags.Domain;

/// <summary>The three targeting levels below an app: site, company and community.</summary>
public enum Level
{
    Site,
    Company,
    Community
}

public sealed record Site(string Id, string Name, string Short, IReadOnlyList<string> CompanyIds, IReadOnlyList<string> CommunityIds);

public sealed record Company(string Id, string Name, string SiteId, IReadOnlyList<string> CommunityIds);

public sealed record Community(string Id, string Name, string City, string SiteId, string CompanyId);

/// <summary>
/// The tenant tree every flag targets: site, then company, then community.
/// Fake data that mirrors the seven ALIS sites.
/// </summary>
public sealed class Hierarchy
{
    private readonly Dictionary<string, Site> _sites;
    private readonly Dictionary<string, Company> _companies;
    private readonly Dictionary<string, Community> _communities;

    private Hierarchy(IReadOnlyList<Site> sites, IReadOnlyList<Company> companies, IReadOnlyList<Community> communities)
    {
        Sites = sites;
        Companies = companies;
        Communities = communities;
        _sites = sites.ToDictionary(s => s.Id);
        _companies = companies.ToDictionary(c => c.Id);
        _communities = communities.ToDictionary(c => c.Id);
    }

    public IReadOnlyList<Site> Sites { get; }
    public IReadOnlyList<Company> Companies { get; }
    public IReadOnlyList<Community> Communities { get; }
    public int TotalCommunities => Communities.Count;

    public Site Site(string id) => _sites[id];
    public Company Company(string id) => _companies[id];
    public Community Community(string id) => _communities[id];

    public bool IsSite(string id) => _sites.ContainsKey(id);
    public bool IsCompany(string id) => _companies.ContainsKey(id);
    public bool IsCommunity(string id) => _communities.ContainsKey(id);
    public bool Contains(string id) => IsSite(id) || IsCompany(id) || IsCommunity(id);

    public Level LevelOf(string id) =>
        IsSite(id) ? Level.Site : IsCompany(id) ? Level.Company : Level.Community;

    public string NameOf(string id) =>
        LevelOf(id) switch
        {
            Level.Site => Site(id).Name,
            Level.Company => Company(id).Name,
            _ => Community(id).Name
        };

    /// <summary>Sites, then companies, then communities, each in seed order.</summary>
    public IEnumerable<string> AllIdsInOrder() =>
        Sites.Select(s => s.Id)
            .Concat(Companies.Select(c => c.Id))
            .Concat(Communities.Select(c => c.Id));

    public static Hierarchy Seed()
    {
        var seed = new[]
        {
            S("S-BETA", "Beta", "Beta",
                C("C-SP", "Summit Pointe",
                    ("SP-ASP", "Summit Pointe Aspen Hall", "Denver, CO"),
                    ("SP-LAR", "Summit Pointe Larkspur", "Littleton, CO"),
                    ("SP-GOL", "Summit Pointe Golden", "Golden, CO"))),
            S("S-AHT", "AHT", "AHT",
                C("C-LV", "Lakeview Assisted Living",
                    ("LV-PIN", "Lakeview Pines", "Ocala, FL"),
                    ("LV-SHO", "Lakeview Shores", "Gainesville, FL"),
                    ("LV-OAK", "Lakeview Oaks", "Lake City, FL"))),
            S("S-ENT1", "ENT1", "ENT1",
                C("C-HV", "Harborview Living",
                    ("HV-BAY", "Harborview at Bay Point", "Portland, ME"),
                    ("HV-MAR", "Harborview at Marina", "South Portland, ME"),
                    ("HV-CAS", "Harborview at Casco", "Falmouth, ME"))),
            S("S-PROD", "Prod", "Prod",
                C("C-WB", "Willowbrook Senior Living",
                    ("WB-BRK", "Willowbrook at Brookline", "Brookline, MA"),
                    ("WB-NEW", "Willowbrook at Newton", "Newton, MA"),
                    ("WB-QUI", "Willowbrook at Quincy", "Quincy, MA"),
                    ("WB-SAL", "Willowbrook at Salem", "Salem, MA"))),
            S("S-PROD2", "Prod 2", "Prod 2",
                C("C-BW", "Brightwater Senior Living",
                    ("BW-OAK", "Brightwater at Oak Ridge", "Raleigh, NC"),
                    ("BW-MIL", "Brightwater at Mill Pond", "Durham, NC"),
                    ("BW-CHA", "Brightwater at Chapel Hill", "Chapel Hill, NC"),
                    ("BW-CAR", "Brightwater at Cary Commons", "Cary, NC"),
                    ("BW-WAK", "Brightwater at Wake Forest", "Wake Forest, NC")),
                C("C-MG", "Maple Grove Communities",
                    ("MG-WIL", "Maple Grove at Willow Creek", "Albany, NY"),
                    ("MG-HUD", "Maple Grove at Hudson", "Hudson, NY"),
                    ("MG-ELM", "Maple Grove at Elm Park", "Schenectady, NY"),
                    ("MG-RIV", "Maple Grove at Riverside", "Troy, NY"))),
            S("S-HEA", "Hearth", "Hearth",
                C("C-CR", "Cedar Ridge Senior Care",
                    ("CR-BOI", "Cedar Ridge Boise", "Boise, ID"),
                    ("CR-MER", "Cedar Ridge Meridian", "Meridian, ID"),
                    ("CR-NAM", "Cedar Ridge Nampa", "Nampa, ID"),
                    ("CR-EAG", "Cedar Ridge Eagle", "Eagle, ID"))),
            S("S-BH", "Benton House", "Benton",
                C("C-NW", "Northwind Retirement",
                    ("NW-KIN", "Northwind Kingsport", "Kingsport, TN"),
                    ("NW-KNX", "Northwind Knoxville", "Knoxville, TN"),
                    ("NW-JOH", "Northwind Johnson City", "Johnson City, TN")),
                C("C-AU", "Aurora Senior Residences",
                    ("AU-KNG", "Aurora at Kingwood", "Humble, TX"),
                    ("AU-MIS", "Aurora at Mission Bend", "Houston, TX")))
        };

        var sites = new List<Site>();
        var companies = new List<Company>();
        var communities = new List<Community>();
        foreach (var site in seed)
        {
            var siteCommunityIds = new List<string>();
            foreach (var company in site.Companies)
            {
                var companyCommunities = company.Communities
                    .Select(m => new Community(m.Id, m.Name, m.City, site.Id, company.Id))
                    .ToList();
                communities.AddRange(companyCommunities);
                siteCommunityIds.AddRange(companyCommunities.Select(m => m.Id));
                companies.Add(new Company(company.Id, company.Name, site.Id, companyCommunities.Select(m => m.Id).ToList()));
            }

            sites.Add(new Site(site.Id, site.Name, site.Short, site.Companies.Select(c => c.Id).ToList(), siteCommunityIds));
        }

        return new Hierarchy(sites, companies, communities);
    }

    private static SiteSeed S(string id, string name, string shortName, params CompanySeed[] companies) =>
        new(id, name, shortName, companies);

    private static CompanySeed C(string id, string name, params (string Id, string Name, string City)[] communities) =>
        new(id, name, communities);

    private sealed record SiteSeed(string Id, string Name, string Short, CompanySeed[] Companies);

    private sealed record CompanySeed(string Id, string Name, (string Id, string Name, string City)[] Communities);
}
