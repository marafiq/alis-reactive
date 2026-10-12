using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: an admissions coordinator assigns an incoming resident to a suite type. Choosing a
    // suite (in Tom Select, a component the sandbox brings) quotes its monthly rate and what is
    // open; the coordinator can take the recommended suite, reserve it, and add a companion's suite.
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

    // The companion's suite loads as a partial with its own plan.
    public class CompanionSuiteModel
    {
        public string? CompanionSuiteType { get; set; }
    }

    /// <summary>A question about one suite type to the Rate and Availability endpoints: <c>{suiteType}</c> in the path.</summary>
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
