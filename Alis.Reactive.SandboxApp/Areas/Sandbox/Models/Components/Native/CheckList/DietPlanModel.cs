namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A resident's diet plan as the kitchen keeps it: the dietary restrictions on file.
    /// </summary>
    public class DietPlanModel
    {
        public int ResidentId { get; set; }
        public List<string> DietaryRestrictions { get; set; } = new List<string>();
    }

    /// <summary>The confirmation the server returns once the diet plan is saved.</summary>
    public sealed record DietPlanSaved(string Confirmation);
}
