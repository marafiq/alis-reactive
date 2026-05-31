using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DateValidationValidator : ReactiveValidator<DateValidationModel>
    {
        public DateValidationValidator()
        {
            ClientRule(x => x.AdmissionDate)
                .Required("Admission date is required.");
            ClientRule(x => x.AdmissionDate)
                .GreaterThanOrEqualTo(new DateTime(2020, 1, 1), "Admission date must be on or after January 1, 2020.");
            ClientRule(x => x.DischargeDate)
                .Required("Discharge date is required.");
            ClientRule(x => x.DischargeDate)
                .GreaterThan(x => x.AdmissionDate, "Discharge date must be after admission date.");
        }
    }
}
