namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A coordinator reviewing today's census and saving a resident's care plan.</summary>
    public class CarePlanReviewModel
    {
        public string? CarePlanNotes { get; set; }
    }

    /// <summary>The census the server reports when it is refreshed.</summary>
    public sealed record CensusSummary(string Summary);

    /// <summary>The confirmation the server returns once the care plan is saved.</summary>
    public sealed record CarePlanSaved(string Confirmation);
}
