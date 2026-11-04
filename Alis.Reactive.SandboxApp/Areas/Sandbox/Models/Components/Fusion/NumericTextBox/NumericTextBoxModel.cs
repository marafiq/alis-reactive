namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
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
