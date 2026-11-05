namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class CarePlanReviewModel
    {
        public string? CarePlanNotes { get; set; }
    }

    public sealed record CensusSummary(string Summary);

    public sealed record CarePlanSaved(string Confirmation);
}
