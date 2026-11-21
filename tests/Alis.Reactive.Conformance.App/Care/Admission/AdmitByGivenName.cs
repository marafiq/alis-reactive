using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AdmissionByName
{
    public string? GivenName { get; set; }
}

public sealed record AdmissionNamesSent(string SentNames, string Admitted)
{
    public static AdmissionNamesSent Of(IEnumerable<string> sentNames, AdmissionByName admission)
        => new(string.Join(", ", sentNames), $"admitted {admission.GivenName}".TrimEnd());
}

public static class AdmitByGivenNameEndpoints
{
    public const string Admissions = CarePages.AdmitByGivenName + "/admissions";
}

public static class AdmitByGivenNames
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
