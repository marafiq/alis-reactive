using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Rating
{
    /// <summary>Margaret Hale's answer to the monthly care satisfaction survey.</summary>
    public class SatisfactionSurveyModel
    {
        /// <summary>The stars she gave last month.</summary>
        public double LastMonthScore { get; set; }

        public double SatisfactionScore { get; set; }

        /// <summary>What made the month harder, asked when she lowers her rating.</summary>
        public string? WhatChanged { get; set; }
    }

    // A survey is not sent unrated. The rating commits with valueChanged, so the form checks it again on that event.
    public class SatisfactionSurveyValidator : ReactiveValidator<SatisfactionSurveyModel>
    {
        public SatisfactionSurveyValidator()
        {
            RuleFor(x => x.SatisfactionScore)
                .GreaterThanOrEqualTo(1d).WithMessage("Choose at least one star to rate October's care.");
        }
    }

    public sealed class SurveyResponse
    {
        public double SatisfactionScore { get; set; }

        public string? WhatChanged { get; set; }
    }

    public sealed class SurveyAnswer
    {
        public string Message { get; set; } = "";
    }
}
