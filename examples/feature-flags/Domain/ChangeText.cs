namespace FeatureFlags.Domain;

/// <summary>Plain-language history lines: what changed between two versions of a flag.</summary>
public sealed class ChangeText(Hierarchy hierarchy)
{
    public IReadOnlyList<string> Lines(Flag before, Flag after)
    {
        var lines = new List<string>();
        if (before.On != after.On) lines.Add(after.On ? "Switched on" : "Switched off for everyone");
        if (before.Name.Length > 0 && before.Name != after.Name) lines.Add("Renamed to " + after.Name);
        if (before.Name.Length > 0 && before.Description != after.Description) lines.Add("Description changed");
        if (before.Everyone != after.Everyone) lines.Add(after.Everyone ? "Turned on for everyone" : "No longer on for everyone");

        var added = PickedIn(after).Where(id => !PickedIn(before).Contains(id)).ToList();
        var removed = PickedIn(before).Where(id => !PickedIn(after).Contains(id)).ToList();
        var kept = Ordered(after.KeepOff).Where(id => !before.KeepOff.Contains(id)).ToList();
        var unkept = Ordered(before.KeepOff).Where(id => !after.KeepOff.Contains(id)).ToList();

        if (added.Count > 0) lines.Add("Turned on for " + Names(added));
        if (removed.Count > 0) lines.Add("Removed " + Names(removed));
        if (kept.Count > 0) lines.Add("Kept off " + Names(kept));
        if (unkept.Count > 0) lines.Add("No longer kept off " + Names(unkept));
        return lines;
    }

    public string Created(Flag flag, string? copiedFromApp)
    {
        var head = "Created"
            + (copiedFromApp is null ? "" : " as a copy from " + Apps.Get(copiedFromApp).Name)
            + (flag.On ? "" : ", switched off");
        var blank = flag with { Name = "", Description = "", On = flag.On, Everyone = false, Sites = Flag.Set(null), Companies = Flag.Set(null), Communities = Flag.Set(null), KeepOff = Flag.Set(null) };
        return string.Join(" · ", new[] { head }.Concat(Lines(blank, flag)));
    }

    /// <summary>"Beta, Summit Pointe and 2 more", with "every site" when all sites are listed.</summary>
    public string Names(IReadOnlyList<string> ids)
    {
        var siteIds = hierarchy.Sites.Select(s => s.Id).ToList();
        var everySite = siteIds.All(id => ids.Contains(id));
        var names = everySite
            ? new[] { "every site" }.Concat(ids.Where(id => !siteIds.Contains(id)).Select(hierarchy.NameOf)).ToList()
            : ids.Select(hierarchy.NameOf).ToList();

        if (names.Count > 4) names = names.Take(3).Append($"{names.Count - 3} more").ToList();
        return names.Count < 2 ? string.Concat(names) : string.Join(", ", names.Take(names.Count - 1)) + " and " + names[^1];
    }

    private List<string> PickedIn(Flag flag) =>
        hierarchy.Sites.Select(s => s.Id).Where(flag.Sites.Contains)
            .Concat(hierarchy.Companies.Select(c => c.Id).Where(flag.Companies.Contains))
            .Concat(hierarchy.Communities.Select(c => c.Id).Where(flag.Communities.Contains))
            .ToList();

    private IEnumerable<string> Ordered(IReadOnlySet<string> ids) =>
        hierarchy.AllIdsInOrder().Where(ids.Contains);
}
