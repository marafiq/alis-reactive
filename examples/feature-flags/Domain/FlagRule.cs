namespace FeatureFlags.Domain;

/// <summary>Why a community has, or does not have, a flag.</summary>
public sealed record Reason(bool Has, string Text, bool KeptOff);

public enum Category
{
    Everyone,
    Some,
    Off
}

/// <summary>
/// The rule within one app: a community has the flag when the flag is on, the community
/// is picked (everyone, or the community, its company or its site) and nothing covering
/// it is kept off. Same rule the SDK evaluates.
/// </summary>
public sealed class FlagRule(Hierarchy hierarchy)
{
    public Reason Why(string communityId, Flag flag)
    {
        var community = hierarchy.Community(communityId);
        if (!flag.On) return new Reason(false, "Flag is off", false);
        if (flag.KeepOff.Contains(communityId)) return new Reason(false, "Kept off", true);
        if (flag.KeepOff.Contains(community.CompanyId)) return new Reason(false, "Company kept off", true);
        if (flag.KeepOff.Contains(community.SiteId)) return new Reason(false, "Site kept off", true);
        if (flag.Everyone) return new Reason(true, "Everyone", false);
        if (flag.Communities.Contains(communityId)) return new Reason(true, "Picked", false);
        if (flag.Companies.Contains(community.CompanyId)) return new Reason(true, "Whole company", false);
        if (flag.Sites.Contains(community.SiteId)) return new Reason(true, "Whole site", false);
        return new Reason(false, "Not picked", false);
    }

    public int CountHas(Flag flag) =>
        hierarchy.Communities.Count(c => Why(c.Id, flag).Has);

    public int SiteCount(Flag flag, string siteId) =>
        hierarchy.Site(siteId).CommunityIds.Count(c => Why(c, flag).Has);

    /// <summary>How many would have it if the flag were switched on.</summary>
    public int CountIfOn(Flag flag) => CountHas(flag with { On = true });

    public Category CategoryOf(Flag flag)
    {
        if (!flag.On) return Category.Off;
        return CountHas(flag) == hierarchy.TotalCommunities ? Category.Everyone : Category.Some;
    }

    /// <summary>"On · 9 of 26 communities", "Off for everyone", and so on.</summary>
    public string StatusOf(Flag flag)
    {
        var total = hierarchy.TotalCommunities;
        var has = CountHas(flag);
        if (!flag.On) return "Off for everyone";
        if (has == total) return "On for everyone";
        if (flag.Everyone) return $"On for everyone except {total - has}";
        if (has == 0) return "On · nobody picked yet";
        return $"On · {has} of {total} communities";
    }

    /// <summary>Fill of one site's bar, 0 to 100, and whether the flag is switched on at all.</summary>
    public SiteFill FillOf(Flag flag, string siteId)
    {
        var site = hierarchy.Site(siteId);
        var on = SiteCount(flag, siteId);
        return new SiteFill(flag.On, on, site.CommunityIds.Count);
    }
}

public sealed record SiteFill(bool FlagOn, int On, int Of)
{
    public int Percent => Of == 0 ? 0 : (int)Math.Round(On * 100.0 / Of);

    /// <summary>CSS modifier for the bar: off, none, full or part.</summary>
    public string State =>
        !FlagOn ? "off"
        : On == Of ? "full"
        : On == 0 ? "none"
        : "part";
}
