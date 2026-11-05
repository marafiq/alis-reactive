
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class StayReservationValidator : ReactiveValidator<DateRangePickerModel>
    {
        public StayReservationValidator()
        {
            RuleFor(x => x.StayPeriod)
                .NotEmpty().WithMessage("Choose the stay dates.")
                .Must(period => period is not [var arrives, var leaves] || (leaves.Date - arrives.Date).Days + 1 <= 30)
                .WithMessage("A respite stay covers at most 30 days. Book a longer stay as an admission.");
        }
    }
}
