namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    // Journey: a care coordinator sets a resident's Monthly Service Plan — how many
    // catered meals per week and how many wellness check-ins per week the resident receives.
    public class NumericTextBoxModel
    {
        public decimal MealsPerWeek { get; set; }

        public decimal WellnessChecksPerWeek { get; set; }
    }

    public sealed class ServicePlanRequest
    {
        public decimal MealsPerWeek { get; set; }
    }

    public sealed class ServicePlanResponse
    {
        public decimal MealsPerWeek { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
