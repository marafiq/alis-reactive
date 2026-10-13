using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RatingModel
    {
        public double SatisfactionScore { get; set; }
    }

    // A survey is not sent unrated. The rating commits with valueChanged, not change: the form checks
    // the rating again on that event because the component declares it as its commit event.
    public class RatingSurveyValidator : ReactiveValidator<RatingModel>
    {
        public RatingSurveyValidator()
        {
            RuleFor(x => x.SatisfactionScore)
                .GreaterThanOrEqualTo(1d).WithMessage("Choose at least one star to rate your care.");
        }
    }

    public sealed class RatingEchoRequest
    {
        public double SatisfactionScore { get; set; }
    }

    public sealed class RatingEchoResponse
    {
        public double SatisfactionScore { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
