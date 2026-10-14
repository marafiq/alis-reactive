using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// An admission day falls while Maple 118 is held for Dorothy, and on a weekday: the admissions office
    /// does not admit at weekends, a check only the server makes.
    /// </summary>
    public class AdmissionDayValidator : ReactiveValidator<FusionDatePickerModel>
    {
        public AdmissionDayValidator()
        {
            RuleFor(x => x.AdmissionDay)
                .NotEmpty().WithMessage("Choose the admission day.")
                .GreaterThanOrEqualTo(FusionDatePickerModel.RoomReady).WithMessage("Maple 118 is ready from Monday, November 2.")
                .LessThanOrEqualTo(FusionDatePickerModel.HoldEnds).WithMessage("Maple 118 is held for Dorothy through Monday, November 30.")
                .Must(day => day is not { DayOfWeek: DayOfWeek.Saturday or DayOfWeek.Sunday })
                .WithMessage("The admissions office admits on weekdays only.");
        }
    }
}
