namespace FeatureFlags.Domain;

/// <summary>An app is the top level above site. Every flag belongs to exactly one app.</summary>
public sealed record AppInfo(string Code, string Name, string Sub);

public static class Apps
{
    public const string Web = "WEB";
    public const string Admin = "ADMIN";
    public const string Daemons = "DAEMONS";

    public static IReadOnlyList<AppInfo> All { get; } =
    [
        new(Web, "ALIS web", "Pages, APIs and the mobile web app"),
        new(Admin, "Admin", "The admin site"),
        new(Daemons, "Daemons", "Background jobs and services")
    ];

    public static AppInfo Get(string code) => All.First(a => a.Code == code);

    public static bool Exists(string code) => All.Any(a => a.Code == code);
}

/// <summary>
/// One flag in one app: its switch and its rollout. The same key may exist in several
/// apps; each copy is a separate flag with its own switch and rollout.
/// </summary>
public sealed record Flag
{
    public required string Id { get; init; }
    public required string App { get; init; }
    public required string Name { get; init; }
    public required string Key { get; init; }
    public string Description { get; init; } = "";
    public bool On { get; init; }
    public bool Everyone { get; init; }
    public IReadOnlySet<string> Sites { get; init; } = new HashSet<string>();
    public IReadOnlySet<string> Companies { get; init; } = new HashSet<string>();
    public IReadOnlySet<string> Communities { get; init; } = new HashSet<string>();
    public IReadOnlySet<string> KeepOff { get; init; } = new HashSet<string>();

    /// <summary>True when name, description, switch and rollout all match.</summary>
    public bool SameSettingsAs(Flag other) =>
        App == other.App
        && Name == other.Name
        && Key == other.Key
        && Description == other.Description
        && On == other.On
        && Everyone == other.Everyone
        && Sites.SetEquals(other.Sites)
        && Companies.SetEquals(other.Companies)
        && Communities.SetEquals(other.Communities)
        && KeepOff.SetEquals(other.KeepOff);

    public static IReadOnlySet<string> Set(IEnumerable<string>? ids) =>
        new HashSet<string>(ids ?? [], StringComparer.Ordinal);
}

public sealed record HistoryEntry(string When, string Who, string What);
