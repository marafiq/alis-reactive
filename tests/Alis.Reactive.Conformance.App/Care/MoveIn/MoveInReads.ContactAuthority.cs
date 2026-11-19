using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInReads
{
    private const string NoDay = "none";

    internal IReadOnlyList<SelectListItem> Roles()
        => [new("Ordinary", nameof(AuthorityRole.Ordinary)), new("Power of attorney", nameof(AuthorityRole.PowerOfAttorney)), new("Guardian", nameof(AuthorityRole.Guardian))];

    internal string[] AdmittedResidents() => [.. Desk.AdmittedResidents()];

    internal bool HoldsRevoked(string reference) => Desk.HoldsRevoked(reference);

    internal string StartOf(AuthorityInput authority) => Day(authority.StartsOn);

    internal string? EndOf(AuthorityInput authority) => authority.EndsOn is { } ends ? Day(ends) : null;

    internal IReadOnlyList<string> StartsOnOffer(AuthorityInput authority) => [.. DaysOnOffer().Append(Day(authority.StartsOn)).Distinct().Order(StringComparer.Ordinal)];

    internal IReadOnlyList<string> EndsOnOffer(AuthorityInput authority)
    {
        var days = authority.EndsOn is { } ends ? DaysOnOffer().Append(Day(ends)) : DaysOnOffer();
        return [NoDay, .. days.Distinct().Order(StringComparer.Ordinal)];
    }

    internal string OnFile(AuthorityOnFilePage held)
    {
        if (held.Authority is not { } authority) return "No authority is on file for this contact.";

        var role = held.Role == AuthorityRole.Guardian ? "guardian" : "power of attorney";
        var days = authority.EndsOn is { } ends ? $"from {Day(authority.StartsOn)} to {Day(ends)}" : $"from {Day(authority.StartsOn)}, with no end";
        var consent = authority.Consent ? "consent given" : "consent not given";
        return $"{held.Contact}: {role} {authority.Reference}, {days}, {consent}";
    }

    private IEnumerable<string> DaysOnOffer()
    {
        var today = Desk.Today();
        DateTime[] days = [new DateTime(today.Year, 1, 1), today.AddDays(-1), today, new DateTime(today.Year, 12, 31), new DateTime(today.Year + 1, 1, 1)];
        return days.Select(Day);
    }
}
