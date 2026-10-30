using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InPlaceEditor
{
    /// <summary>Eleanor Vance's profile, saved together with Save profile.</summary>
    public class ResidentProfileModel
    {
        public string? Name { get; set; }

        public DateTime? AdmissionDate { get; set; }

        /// <summary>The name on her door sign and the one staff call her by.</summary>
        public string? PreferredName { get; set; }

        /// <summary>A profile under care review keeps its preferred name.</summary>
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

    /// <summary>What the profile answers when it is saved.</summary>
    public sealed class ProfileSaved
    {
        public string Message { get; set; } = "";
    }
}
