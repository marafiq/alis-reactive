namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class CarePlanLookupModel
    {
        public string? ResidentId { get; set; }
    }

    public sealed class CarePlanResident
    {
        public string? ResidentId { get; set; }
    }

    public sealed record CarePlanResidentOption(string Id, string Name);

    public sealed record CarePlanSummary(string Resident, string Plan, string Alerts);
}
