using System.Globalization;

namespace FeatureFlags.Domain;

/// <summary>
/// In-memory flag store seeded with fake flags. Stands in for the database the real
/// SDK reads; everything here resets when the app restarts.
/// </summary>
public sealed class FlagStore
{
    private readonly object _gate = new();
    private readonly ChangeText _changeText;
    private readonly List<string> _order = [];
    private readonly Dictionary<string, Flag> _flags = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<HistoryEntry>> _history = new(StringComparer.Ordinal);
    private readonly HashSet<string> _createdHere = new(StringComparer.Ordinal);
    private int _nextId = 1;

    public FlagStore(ChangeText changeText)
    {
        _changeText = changeText;
        Seed();
    }

    public IReadOnlyList<Flag> All()
    {
        lock (_gate) return _order.Select(id => _flags[id]).ToList();
    }

    public IReadOnlyList<Flag> InApp(string app) =>
        All().Where(f => f.App == app).ToList();

    public Flag? Find(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        lock (_gate) return _flags.GetValueOrDefault(id);
    }

    public Flag? FirstInApp(string app) => InApp(app).FirstOrDefault();

    public IReadOnlyList<HistoryEntry> History(string id)
    {
        lock (_gate) return _history.TryGetValue(id, out var entries) ? entries.ToList() : [];
    }

    /// <summary>Flags created since the app started get a NEW badge in the list.</summary>
    public bool IsCreatedHere(string id)
    {
        lock (_gate) return _createdHere.Contains(id);
    }

    /// <summary>The flag with the same key in another app, if one exists.</summary>
    public Flag? TwinIn(Flag flag, string app) =>
        All().FirstOrDefault(f => f.App == app && f.Key == flag.Key && f.Id != flag.Id);

    public IReadOnlyList<Flag> TwinsOf(Flag flag) =>
        All().Where(f => f.Key == flag.Key && f.App != flag.App).ToList();

    /// <summary>Why a key cannot be used in an app, or null when it can.</summary>
    public string? KeyProblem(string app, string key, string? exceptId)
    {
        var trimmed = key.Trim();
        if (trimmed.Length == 0) return "Add a key.";
        if (!FlagKeys.HasValidShape(trimmed)) return FlagKeys.PatternMessage;
        var clash = All().FirstOrDefault(f => f.App == app && f.Key == trimmed && f.Id != exceptId);
        return clash is null ? null : $"Already used in {Apps.Get(app).Name} by {clash.Name}.";
    }

    public Flag Save(Flag updated)
    {
        lock (_gate)
        {
            var before = _flags[updated.Id];
            var lines = _changeText.Lines(before, updated);
            var what = lines.Count == 0 ? "No changes" : string.Join(" · ", lines);
            _history[updated.Id].Insert(0, new HistoryEntry(Today(), "You", what));
            _flags[updated.Id] = updated;
            return updated;
        }
    }

    public Flag Create(Flag draft, string? copiedFromApp)
    {
        lock (_gate)
        {
            var created = draft with { Id = "f" + _nextId++ };
            _flags[created.Id] = created;
            _order.Insert(0, created.Id);
            _createdHere.Add(created.Id);
            _history[created.Id] = [new HistoryEntry(Today(), "You", _changeText.Created(created, copiedFromApp))];
            return created;
        }
    }

    private static string Today() => DateTime.Now.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    private void Seed()
    {
        Add(F("emar", Apps.Web, "eMAR barcode scanning v2", "emar.barcode-scan-v2", "Nurses scan the resident wristband and the medication package at med pass.", true, false, ["S-BETA"], ["C-BW"], ["MG-WIL", "MG-RIV"], ["BW-MIL"]),
            [
                new("Sep 29, 2026", "L. Okafor", "Turned on for Maple Grove at Willow Creek and Maple Grove at Riverside"),
                new("Sep 24, 2026", "L. Okafor", "Kept off Brightwater at Mill Pond"),
                new("Sep 22, 2026", "L. Okafor", "Turned on for Brightwater Senior Living"),
                new("Aug 4, 2026", "P. Singh", "Created · Turned on for Beta")
            ]);
        Add(F("sched", Apps.Web, "Scheduling on .NET 10", "platform.scheduling-net10", "Staff scheduling served by the new .NET 10 module.", true, false, ["S-BETA", "S-PROD2"]), "Oct 7, 2026", "P. Singh");
        Add(F("incident", Apps.Web, "Incident report redesign", "incidents.form-v3", "Guided incident form with required follow-up tasks.", true, true, keepOff: ["BW-MIL", "LV-PIN"]), "Oct 1, 2026", "P. Singh");
        Add(F("facesheet", Apps.Web, "Resident face sheet v2", "resident.facesheet-v2", "One-page face sheet with allergies, code status and contacts.", true, true), "Sep 18, 2026", "P. Singh");
        Add(F("family", Apps.Web, "Family portal messaging", "family.messaging", "Two-way messages between families and community staff.", true, false, communities: ["BW-OAK", "MG-WIL", "SP-LAR"]), "Sep 30, 2026", "D. Mensah");
        Add(F("notes", Apps.Web, "Care note summaries", "care.ai-note-summary", "Shows the overnight shift summary on the resident page.", true, false, ["S-BETA"], communities: ["BW-OAK"]), "Oct 5, 2026", "L. Okafor");
        Add(F("ach", Apps.Web, "ACH autopay", "billing.ach-autopay", "Families set up recurring bank drafts in the portal.", false, false, companies: ["C-BW", "C-CR"]), "Oct 2, 2026", "R. Alvarez");
        Add(F("legacy", Apps.Web, "Legacy report export", "reports.legacy-export", "Old CSV export, replaced by Report Builder.", false, false), "Jul 30, 2026", "P. Singh");
        Add(F("tenant", Apps.Admin, "Tenant setup wizard", "admin.tenant-setup-v2", "Guided setup for a new company and its communities.", true, true), "Sep 12, 2026", "R. Alvarez");
        Add(F("bulk", Apps.Admin, "Bulk user import", "admin.bulk-user-import", "Import staff accounts from a spreadsheet.", true, false, ["S-BETA", "S-AHT"]), "Oct 6, 2026", "R. Alvarez");
        Add(F("emar-d", Apps.Daemons, "eMAR barcode scanning v2", "emar.barcode-scan-v2", "Med pass reminder job uses the scanned-dose log. Trails the web rollout.", true, false, ["S-BETA"], ["C-BW"], keepOff: ["BW-MIL"]), "Sep 25, 2026", "P. Singh", Apps.Web);
        Add(F("notes-d", Apps.Daemons, "Care note summaries", "care.ai-note-summary", "Overnight job that drafts shift summaries from the last 24 hours of care notes.", true, false, ["S-BETA"], communities: ["BW-OAK"]), "Oct 5, 2026", "L. Okafor", Apps.Web);
        Add(F("ach-d", Apps.Daemons, "ACH autopay", "billing.ach-autopay", "Nightly job that submits recurring bank drafts.", false, false, companies: ["C-BW"]), "Oct 2, 2026", "R. Alvarez", Apps.Web);
        Add(F("queue", Apps.Daemons, "Queue processor on .NET 10", "jobs.queue-net10", "Background queue handled by the new .NET 10 worker.", true, false, ["S-BETA", "S-AHT", "S-ENT1"]), "Oct 3, 2026", "P. Singh");
    }

    private void Add(Flag flag, string when, string who, string? copiedFromApp = null) =>
        Add(flag, [new HistoryEntry(when, who, _changeText.Created(flag, copiedFromApp))]);

    private void Add(Flag flag, List<HistoryEntry> history)
    {
        _flags[flag.Id] = flag;
        _order.Add(flag.Id);
        _history[flag.Id] = history;
    }

    private static Flag F(
        string id, string app, string name, string key, string description, bool on, bool everyone,
        string[]? sites = null, string[]? companies = null, string[]? communities = null, string[]? keepOff = null) =>
        new()
        {
            Id = id,
            App = app,
            Name = name,
            Key = key,
            Description = description,
            On = on,
            Everyone = everyone,
            Sites = Flag.Set(sites),
            Companies = Flag.Set(companies),
            Communities = Flag.Set(communities),
            KeepOff = Flag.Set(keepOff)
        };
}
