namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Rating
{
    public class SatisfactionSurveyModel
    {
        public double LastMonthScore { get; set; }

        public double SatisfactionScore { get; set; }

        public string? WhatChanged { get; set; }
    }

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
