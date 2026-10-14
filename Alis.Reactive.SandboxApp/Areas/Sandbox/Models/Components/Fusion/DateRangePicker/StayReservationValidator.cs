using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A respite stay is reserved for the dates the coordinator picks, and covers at most 30 days: a longer
    /// stay is an admission, a check only the server makes.
    /// </summary>
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
