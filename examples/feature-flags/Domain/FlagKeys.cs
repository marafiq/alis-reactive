using System.Text.RegularExpressions;

namespace FeatureFlags.Domain;

public sealed record Snippet(string Lang, string Code);

/// <summary>Keys are what code checks. They are fixed once a flag exists.</summary>
public static partial class FlagKeys
{
    public const string Pattern = "^[a-z0-9][a-z0-9.-]*$";
    public const string PatternMessage = "Use lowercase letters, numbers, dots and dashes.";

    [GeneratedRegex(Pattern)]
    private static partial Regex KeyShape();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlug();

    public static bool HasValidShape(string key) => KeyShape().IsMatch(key);

    /// <summary>"Fall risk alerts" becomes "fall-risk-alerts".</summary>
    public static string Slug(string? name) =>
        NonSlug().Replace((name ?? "").ToLowerInvariant(), "-").Trim('-');

    /// <summary>"care.fall-risk-alerts" becomes "CareFallRiskAlerts".</summary>
    public static string ConstName(string key)
    {
        var words = key.Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + w[1..]);
        var name = string.Concat(words);
        return name.Length == 0 ? "MyFlag" : name;
    }

    /// <summary>Copy-ready calls for one app. Web and Admin check per request; Daemons pass a scope.</summary>
    public static IReadOnlyList<Snippet> Snippets(string app, string key)
    {
        var shownKey = string.IsNullOrWhiteSpace(key) ? "your-flag-key" : key.Trim();
        var constant = ConstName(shownKey);
        var snippets = new List<Snippet>
        {
            new("Key", shownKey),
            new("Const", $"public const string {constant} = \"{shownKey}\";")
        };

        if (app == Apps.Daemons)
        {
            snippets.Add(new("Community", $"flags.For(FlagScope.Community(site, companyId, communityId)).IsOn(Flags.{constant})"));
            snippets.Add(new("Site", $"flags.For(FlagScope.Site(site)).IsOn(Flags.{constant})"));
            snippets.Add(new("App", $"flags.For(FlagScope.App).IsOn(Flags.{constant})"));
            return snippets;
        }

        snippets.Add(new("C#", $"flags.IsOn(Flags.{constant})"));
        snippets.Add(new("Razor", $"@if (Html.FeatureOn(Flags.{constant})) {{ … }}"));
        snippets.Add(new("JS", $"alisFlags.isOn('{shownKey}')"));
        return snippets;
    }
}
