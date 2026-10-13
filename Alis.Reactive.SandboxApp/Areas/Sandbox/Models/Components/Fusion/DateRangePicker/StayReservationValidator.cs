using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A respite stay is reserved for the dates the coordinator picks on the stay period.</summary>
    public class StayReservationValidator : ReactiveValidator<DateRangePickerModel>
    {
        public StayReservationValidator()
        {
            RuleFor(x => x.StayPeriod).NotEmpty().WithMessage("Choose the stay dates.");
        }
    }
}
