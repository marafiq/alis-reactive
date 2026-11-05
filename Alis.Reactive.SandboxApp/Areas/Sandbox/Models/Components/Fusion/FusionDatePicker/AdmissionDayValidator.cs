
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
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
