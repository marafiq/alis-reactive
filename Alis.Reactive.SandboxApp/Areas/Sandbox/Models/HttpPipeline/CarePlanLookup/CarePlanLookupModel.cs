namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Care plan lookup: the nurse chooses a resident and opens their care plan, fetched by the resident's
    /// id in the URL.
    /// </summary>
    public class CarePlanLookupModel
    {
        /// <summary>The resident number chosen; null until the nurse chooses one.</summary>
        public string? ResidentId { get; set; }
    }

    /// <summary>A resident the nurse can choose.</summary>
    public sealed record CarePlanResidentOption(string Id, string Name);

    /// <summary>A resident's care plan, as the server sends it.</summary>
    public sealed record CarePlanSummary(string Resident, string Plan);
}
