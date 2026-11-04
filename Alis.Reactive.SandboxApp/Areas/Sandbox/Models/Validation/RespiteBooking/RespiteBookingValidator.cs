namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RespiteBookingValidator : ReactiveValidator<RespiteBookingModel>
    {
        private static readonly List<int?> CommunitiesWithTransportDesk = new() { 1, 3 };

        public RespiteBookingValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Resident name is required.");
            RuleFor(x => x.CommunityId).NotEmpty().WithMessage("Choose a community.");
            RuleFor(x => x.DailyRate).NotEmpty().WithMessage("Daily rate is required.");

            When(x => x.ExpectedNights == null, () =>
                RuleFor(x => x.EndOfStayPlan).NotEmpty().WithMessage("An open-ended stay needs a plan for how it ends."));

            When(x => x.ExpectedNights.HasValue, () =>
                RuleFor(x => x.LastDayPickupTime).NotEmpty().WithMessage("A planned stay needs the pickup time on its last day."));

            When(x => !CommunitiesWithTransportDesk.Contains(x.CommunityId), () =>
                RuleFor(x => x.NeedsTransport).NotNull().WithMessage("This community has no transport desk: answer whether it drives the resident in."));
        }
    }
}
