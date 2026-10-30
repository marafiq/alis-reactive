using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The follow-up falls inside the window Dr. Osei set, and inside her clinic hours, a check only the
    /// server makes.
    /// </summary>
    public class FollowUpAppointmentValidator : ReactiveValidator<DateTimePickerModel>
    {
        public FollowUpAppointmentValidator()
        {
            RuleFor(x => x.FollowUpAppointment)
                .NotEmpty().WithMessage("Choose the appointment time.")
                .GreaterThanOrEqualTo(DateTimePickerModel.FirstOpening).WithMessage("Dr. Osei's first opening after the hospital stay is Monday, November 9.")
                .LessThan(DateTimePickerModel.WindowCloses).WithMessage("Dr. Osei wants to see Arthur by Friday, November 20.")
                .Must(time => time is not { } visit || DateTimePickerModel.InClinicHours(visit))
                .WithMessage("Dr. Osei sees residents on weekdays from 9:00 AM to 4:00 PM.");
        }
    }
}
