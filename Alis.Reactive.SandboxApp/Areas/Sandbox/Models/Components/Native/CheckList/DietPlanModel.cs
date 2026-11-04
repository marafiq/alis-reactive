namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DietPlanModel
    {
        public int ResidentId { get; set; }
        public List<string> DietaryRestrictions { get; set; } = new List<string>();
    }

    public sealed record DietPlanSaved(string Confirmation);
}
