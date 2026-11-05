namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    public class SuiteAssignmentModel
    {
        public string? SuiteType { get; set; }

        public decimal MonthlyRate { get; set; }
    }

    public class SuiteAssignmentValidator : ReactiveValidator<SuiteAssignmentModel>
    {
        public SuiteAssignmentValidator()
        {
            RuleFor(x => x.SuiteType).NotEmpty().WithMessage("Choose a suite type.");
        }
    }

    public class CompanionSuiteModel
    {
        public string? CompanionSuiteType { get; set; }
    }

    public sealed class SuiteQuery
    {
        public string SuiteType { get; set; } = string.Empty;
    }

    public sealed class SuiteRateResponse
    {
        public string SuiteType { get; set; } = string.Empty;

        public decimal MonthlyRate { get; set; }
    }

    public sealed class SuiteAvailabilityResponse
    {
        public string Summary { get; set; } = string.Empty;
    }

    public sealed class SuiteRecommendationResponse
    {
        public string SuiteType { get; set; } = string.Empty;
    }

    public sealed class SuiteReservationResponse
    {
        public string Confirmation { get; set; } = string.Empty;
    }

    public sealed class MoveInDocumentResponse
    {
        public string Title { get; set; } = string.Empty;
    }
}
