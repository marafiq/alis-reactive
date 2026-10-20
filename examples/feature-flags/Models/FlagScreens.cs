using FeatureFlags.Domain;

namespace FeatureFlags.Models;

/// <summary>
/// Builds what each part of the page shows. The server answers every question about a
/// flag (who has it, what saving would change) from the posted draft; the browser only
/// shows the answer.
/// </summary>
public sealed class FlagScreens(Hierarchy hierarchy, FlagRule rule, FlagStore store)
{
    public const string NoSite = "none";

    // ── Flag list ───────────────────────────────────────────────────────────

    public FlagListScreen List(FlagsPageModel page)
    {
        var inApp = store.InApp(page.App);
        var counts = new Dictionary<string, int>
        {
            [FlagFilters.All] = inApp.Count,
            [FlagFilters.Everyone] = inApp.Count(f => rule.CategoryOf(f) == Category.Everyone),
            [FlagFilters.Some] = inApp.Count(f => rule.CategoryOf(f) == Category.Some),
            [FlagFilters.Off] = inApp.Count(f => rule.CategoryOf(f) == Category.Off)
        };

        var query = (page.Query ?? "").Trim().ToLowerInvariant();
        var rows = inApp
            .Where(f => query.Length == 0 || f.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || f.Key.Contains(query, StringComparison.Ordinal))
            .Where(f => page.Filter == FlagFilters.All || CategoryName(rule.CategoryOf(f)) == page.Filter)
            .Select(Row)
            .ToList();

        var tabs = Apps.All.Select(a => new AppTab(a.Code, a.Name, store.InApp(a.Code).Count)).ToList();
        return new FlagListScreen(tabs, counts, rows);
    }

    private FlagRow Row(Flag flag)
    {
        var has = rule.CountHas(flag);
        var dot = !flag.On ? "off" : has > 0 ? "on" : "empty";
        var last = store.History(flag.Id).First();
        var alsoIn = store.TwinsOf(flag).Select(t => "Also in " + Apps.Get(t.App).Name).ToList();
        return new FlagRow(flag.Id, flag.Name, store.IsCreatedHere(flag.Id), rule.StatusOf(flag), dot, Strip(flag, selectedSite: null), alsoIn, $"Changed {last.When} by {last.Who}");
    }

    private static string CategoryName(Category category) => category switch
    {
        Category.Everyone => FlagFilters.Everyone,
        Category.Some => FlagFilters.Some,
        _ => FlagFilters.Off
    };

    // ── One flag ────────────────────────────────────────────────────────────

    public FlagDetailScreen Detail(FlagEditModel form)
    {
        var saved = store.Find(form.Id);
        var draft = form.ToDraft(saved);
        IReadOnlyList<HistoryEntry> history = saved is null ? [] : store.History(saved.Id);
        var last = history.FirstOrDefault();

        return new FlagDetailScreen(
            Form: form,
            HeaderName: form.IsNew ? (string.IsNullOrWhiteSpace(form.Name) ? "New flag" : form.Name!) : saved!.Name,
            AppName: Apps.Get(draft.App).Name,
            KeyShown: draft.Key.Length == 0 ? "key not set" : draft.Key,
            LastChanged: last is null ? "" : $"Changed {last.When} by {last.Who}",
            Who: WhoHasIt(form),
            Save: SaveSummary(form),
            KeyProblem: KeyProblem(form),
            SiteItems: hierarchy.Sites.Select(s => new PickItem(s.Id, s.Name, "")).ToList(),
            CompanyItems: hierarchy.Companies.Select(c => new PickItem(c.Id, c.Name, hierarchy.Site(c.SiteId).Name)).ToList(),
            CommunityItems: hierarchy.Communities.Select(m => new PickItem(m.Id, m.Name, $"{hierarchy.Site(m.SiteId).Name} · {hierarchy.Company(m.CompanyId).Name}")).ToList(),
            KeepOffItems: KeepOffItems(),
            OtherApps: saved is null ? [] : OtherApps(saved),
            History: history,
            Snippets: FlagKeys.Snippets(draft.App, draft.Key));
    }

    public FlagEditModel NewForm(string app) =>
        new() { App = Apps.Exists(app) ? app : Apps.Web, On = true };

    /// <summary>A copy of a saved flag, ready to create in another app.</summary>
    public FlagEditModel CopyForm(Flag source, string app)
    {
        var form = FlagEditModel.From(source);
        form.Id = "";
        form.App = app;
        form.CopiedFrom = source.App;
        form.CopySource = source.Id;
        form.KeyTouched = "true";
        return form;
    }

    public WhoHasItScreen WhoHasIt(FlagEditModel form)
    {
        var saved = store.Find(form.Id);
        var draft = form.ToDraft(saved);
        var (storedSite, selectedSite) = ResolveSite(draft, form.WhoSite, form.PickSite);

        var total = hierarchy.TotalCommunities;
        var has = rule.CountHas(draft);
        var wouldHave = draft.On ? has : rule.CountIfOn(draft);
        var text = draft.On
            ? $"of {total} communities have it"
            : wouldHave > 0
                ? $"have it · the flag is off. Switch it on and {Plural(wouldHave, "community gets", "communities get")} it."
                : "have it · the flag is off for everyone";

        IReadOnlyList<WhoGroup> groups = selectedSite is null
            ? []
            : hierarchy.Site(selectedSite).CompanyIds.Select(c => new WhoGroup(
                hierarchy.Company(c).Name,
                hierarchy.Company(c).CommunityIds.Select(m =>
                {
                    var reason = rule.Why(m, draft);
                    return new WhoItem(hierarchy.Community(m).Name, reason.Text, Pill.For(reason));
                }).ToList())).ToList();

        var selectedTitle = selectedSite is null
            ? ""
            : $"{hierarchy.Site(selectedSite).Name} · {rule.SiteCount(draft, selectedSite)} of {Plural(hierarchy.Site(selectedSite).CommunityIds.Count, "community", "communities")}";

        var appWideOn = draft.On && draft.Everyone;
        var appWideSub = appWideOn ? "Jobs and admin pages that aren't about one community"
            : !draft.On ? "The flag is off"
            : "On only when the flag is on for everyone";

        return new WhoHasItScreen(
            Count: draft.On ? has : 0,
            Text: text,
            ShowAfterSave: saved is not null && !draft.SameSettingsAs(saved),
            Strip: Strip(draft, selectedSite),
            WhoSite: storedSite,
            SelectedTitle: selectedTitle,
            Groups: groups,
            ShowAppWide: draft.App != Apps.Web,
            AppWideSub: appWideSub,
            AppWidePill: appWideOn ? new Pill("On", "on") : new Pill("Off", "off"));
    }

    public SaveSummaryScreen SaveSummary(FlagEditModel form)
    {
        var saved = store.Find(form.Id);
        var draft = form.ToDraft(saved);

        if (saved is null)
        {
            var sub = form.CopiedFrom.Length > 0 ? $"Copy from {Apps.Get(form.CopiedFrom).Name} · not created yet" : "Not created yet";
            var problem = draft.Name.Length == 0 ? "Add a name to create the flag." : KeyProblem(form);
            return new SaveSummaryScreen("New flag", sub, problem);
        }

        if (draft.SameSettingsAs(saved))
            return new SaveSummaryScreen("No unsaved changes", "Change the rollout above to see the effect here.", "");

        var was = rule.CountHas(saved);
        var has = rule.CountHas(draft);
        var effect = saved.On && !draft.On
            ? $"Turns it off for all {Plural(was, "community", "communities")}"
            : $"{was} → {has} communities";
        return new SaveSummaryScreen("Unsaved changes", effect, "");
    }

    public string KeyProblem(FlagEditModel form) =>
        form.IsNew ? store.KeyProblem(form.App, form.Key ?? "", exceptId: null) ?? "" : "";

    private IReadOnlyList<OtherAppRow> OtherApps(Flag saved) =>
        Apps.All.Where(a => a.Code != saved.App).Select(a =>
        {
            var twin = store.TwinIn(saved, a.Code);
            return twin is null
                ? new OtherAppRow(a.Code, a.Name, null, "No flag with this key", null)
                : new OtherAppRow(a.Code, a.Name, twin.Id, rule.StatusOf(twin), twin.On ? new Pill("On", "on") : new Pill("Off", "off"));
        }).ToList();

    private IReadOnlyList<PickItem> KeepOffItems() =>
        hierarchy.Sites.Select(s => new PickItem(s.Id, s.Name, "Sites"))
            .Concat(hierarchy.Companies.Select(c => new PickItem(c.Id, c.Name, "Companies")))
            .Concat(hierarchy.Communities.Select(m => new PickItem(m.Id, m.Name, "Communities")))
            .ToList();

    private IReadOnlyList<StripCell> Strip(Flag flag, string? selectedSite) =>
        hierarchy.Sites.Select(s =>
        {
            var fill = rule.FillOf(flag, s.Id);
            return new StripCell(s.Id, s.Short, $"{s.Name}: {fill.On} of {fill.Of} communities", fill, s.Id == selectedSite);
        }).ToList();

    /// <summary>
    /// Which site's communities to list. An empty stored value means "pick a useful one"
    /// (the first part-way site, else the first site that has it); "none" means the user
    /// closed it. Tapping the open site closes it.
    /// </summary>
    private (string Stored, string? Selected) ResolveSite(Flag draft, string whoSite, string pickSite)
    {
        var current = whoSite.Length == 0 ? DefaultSite(draft) : whoSite == NoSite ? null : whoSite;
        if (pickSite.Length == 0 || !hierarchy.IsSite(pickSite))
            return (whoSite, current);

        return pickSite == current ? (NoSite, null) : (pickSite, pickSite);
    }

    private string? DefaultSite(Flag draft)
    {
        var partial = hierarchy.Sites.FirstOrDefault(s =>
        {
            var on = rule.SiteCount(draft, s.Id);
            return on > 0 && on < s.CommunityIds.Count;
        });
        return partial?.Id ?? hierarchy.Sites.FirstOrDefault(s => rule.SiteCount(draft, s.Id) > 0)?.Id;
    }

    // ── Community check ─────────────────────────────────────────────────────

    public IReadOnlyList<PickItem> CommunityChoices() =>
        hierarchy.Communities.Select(m => new PickItem(m.Id, $"{m.Name} · {m.City}", $"{hierarchy.Site(m.SiteId).Name} · {hierarchy.Company(m.CompanyId).Name}")).ToList();

    public CheckResultScreen? Check(string app, string communityId)
    {
        if (!hierarchy.IsCommunity(communityId)) return null;

        var community = hierarchy.Community(communityId);
        var rows = store.InApp(app).Select(f =>
        {
            var reason = rule.Why(communityId, f);
            return new CheckRow(f.Id, f.Name, reason.Text, Pill.For(reason));
        }).ToList();
        var onCount = rows.Count(r => r.Pill.Kind == "on");
        return new CheckResultScreen(
            $"{hierarchy.Site(community.SiteId).Name} · {hierarchy.Company(community.CompanyId).Name}",
            $"{onCount} of {Plural(rows.Count, "flag", "flags")} on here in {Apps.Get(app).Name}",
            rows);
    }

    public static string Plural(int n, string one, string many) => $"{n} {(n == 1 ? one : many)}";
}
