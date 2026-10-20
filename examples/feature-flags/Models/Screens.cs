using FeatureFlags.Domain;

namespace FeatureFlags.Models;

/// <summary>"On", "Kept off" or "Off" pill next to a community or a flag.</summary>
public sealed record Pill(string Text, string Kind)
{
    public static Pill For(Reason reason) =>
        reason.Has ? new("On", "on") : reason.KeptOff ? new("Kept off", "out") : new("Off", "off");
}

/// <summary>One site's bar in a strip.</summary>
public sealed record StripCell(string SiteId, string Label, string Title, SiteFill Fill, bool Selected);

// ── Flag list ───────────────────────────────────────────────────────────────

public sealed record AppTab(string Code, string Name, int Count);

public sealed record FlagRow(
    string Id,
    string Name,
    bool IsNew,
    string Status,
    string DotState,
    IReadOnlyList<StripCell> Strip,
    IReadOnlyList<string> AlsoIn,
    string Changed);

public sealed record FlagListScreen(
    IReadOnlyList<AppTab> Apps,
    IReadOnlyDictionary<string, int> FilterCounts,
    IReadOnlyList<FlagRow> Rows);

// ── One flag ────────────────────────────────────────────────────────────────

public sealed record WhoGroup(string Title, IReadOnlyList<WhoItem> Items);

public sealed record WhoItem(string Name, string Reason, Pill Pill);

public sealed record WhoHasItScreen(
    int Count,
    string Text,
    bool ShowAfterSave,
    IReadOnlyList<StripCell> Strip,
    string WhoSite,
    string SelectedTitle,
    IReadOnlyList<WhoGroup> Groups,
    bool ShowAppWide,
    string AppWideSub,
    Pill AppWidePill)
{
    public bool HasSelection => Groups.Count > 0;
}

public sealed record SaveSummaryScreen(string Title, string Sub, string Problem);

public sealed record OtherAppRow(string Code, string Name, string? TwinId, string Status, Pill? Pill);

public sealed record FlagDetailScreen(
    FlagEditModel Form,
    string HeaderName,
    string AppName,
    string KeyShown,
    string LastChanged,
    WhoHasItScreen Who,
    SaveSummaryScreen Save,
    string KeyProblem,
    IReadOnlyList<PickItem> SiteItems,
    IReadOnlyList<PickItem> CompanyItems,
    IReadOnlyList<PickItem> CommunityItems,
    IReadOnlyList<PickItem> KeepOffItems,
    IReadOnlyList<OtherAppRow> OtherApps,
    IReadOnlyList<HistoryEntry> History,
    IReadOnlyList<Snippet> Snippets)
{
    public bool IsNew => Form.IsNew;
    public bool IsCopy => Form.CopiedFrom.Length > 0;
}

// ── Community check ─────────────────────────────────────────────────────────

public sealed record CheckRow(string FlagId, string Name, string Reason, Pill Pill);

public sealed record CheckResultScreen(string Where, string Summary, IReadOnlyList<CheckRow> Rows);
