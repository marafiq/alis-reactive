
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InPlaceEditor
{
    public class ResidentProfileModel
    {
        public string? Name { get; set; }

        public DateTime? AdmissionDate { get; set; }

        public string? PreferredName { get; set; }

        public bool ProfileLocked { get; set; }
    }

    public class ResidentProfileModelValidator : ReactiveValidator<ResidentProfileModel>
    {
        public ResidentProfileModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Enter the resident's name.")
                .MaximumLength(80).WithMessage("Keep the resident's name under 80 characters.");
            RuleFor(x => x.AdmissionDate)
                .NotEmpty().WithMessage("Enter the admission date.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("The admission date cannot be after today.");
            RuleFor(x => x.PreferredName)
                .MaximumLength(30).WithMessage("Keep the preferred name to 30 characters so it fits her door sign.");
        }
    }

    public sealed class ProfileSaved
    {
        public string Message { get; set; } = "";
    }
}
