using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentValidator : ReactiveValidator<ResidentModel>
    {
        public ResidentValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("'Name' is required.")
                .MinimumLength(2).WithMessage("'Name' must have a minimum length of 2.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' must be a valid email address.");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");

            RuleFor(x => x.ConfirmEmail)
                .NotEmpty().WithMessage("'Confirm Email' is required.")
                .Equal(x => x.Email).WithMessage("'Confirm Email' must match 'Email'.");

            When(x => x.IsVeteran, () =>
            {
                RuleFor(x => x.VeteranId).NotEmpty().WithMessage("'Veteran ID' is required when veteran.");
            });

            When(x => x.CareLevel == "Memory Care", () =>
            {
                RuleFor(x => x.MemoryAssessmentScore).NotEmpty().WithMessage("'Memory Assessment' is required for Memory Care.");
            });

            When(x => x.CareLevel != "Independent", () =>
            {
                RuleFor(x => x.PhysicianName).NotEmpty().WithMessage("'Physician' is required unless Independent.");
            });

            When(x => x.HasEmergencyContact, () =>
            {
                RuleFor(x => x.EmergencyName).NotEmpty().WithMessage("'Emergency Name' is required.");
                RuleFor(x => x.EmergencyPhone).NotEmpty().WithMessage("'Emergency Phone' is required.");
            });

            When(x => !x.HasEmergencyContact, () =>
            {
                RuleFor(x => x.ReasonForNoContact).NotEmpty().WithMessage("'Reason' is required when no emergency contact.");
            });

            RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
        }
    }

    // ServerPartial omits rules for fields that page does not render.
    public class ServerPartialValidator : ReactiveValidator<ResidentModel>
    {
        public ServerPartialValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("'Name' is required.")
                .MinimumLength(2).WithMessage("'Name' must have a minimum length of 2.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' must be a valid email address.");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");

            RuleFor(x => x.ConfirmEmail)
                .NotEmpty().WithMessage("'Confirm Email' is required.")
                .Equal(x => x.Email).WithMessage("'Confirm Email' must match 'Email'.");

            When(x => x.IsVeteran, () =>
            {
                RuleFor(x => x.VeteranId).NotEmpty().WithMessage("'Veteran ID' is required when veteran.");
            });

            When(x => x.HasEmergencyContact, () =>
            {
                RuleFor(x => x.EmergencyName).NotEmpty().WithMessage("'Emergency Name' is required.");
                RuleFor(x => x.EmergencyPhone).NotEmpty().WithMessage("'Emergency Phone' is required.");
            });

            When(x => !x.HasEmergencyContact, () =>
            {
                RuleFor(x => x.ReasonForNoContact).NotEmpty().WithMessage("'Reason' is required when no emergency contact.");
            });

            RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
        }
    }

    // AjaxPartial keeps address validation wired across Active Plan partial enrichment.
    public class AjaxPartialValidator : ReactiveValidator<ResidentModel>
    {
        public AjaxPartialValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("'Name' is required.")
                .MinimumLength(2).WithMessage("'Name' must have a minimum length of 2.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' must be a valid email address.");

            RuleFor(x => x.ConfirmEmail)
                .NotEmpty().WithMessage("'Confirm Email' is required.")
                .Equal(x => x.Email).WithMessage("'Confirm Email' must match 'Email'.");

            // Custom Address reports summary errors until the partial loads address fields, then inline errors.
            When(x => x.AddressType == "Custom Address", () =>
            {
                RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
            });
        }
    }

    public class ResidentAddressValidator : ReactiveValidator<ResidentAddress>
    {
        public ResidentAddressValidator()
        {
            RuleFor(x => x.Street).NotEmpty().WithMessage("'Street' is required.");
            RuleFor(x => x.City).NotEmpty().WithMessage("'City' is required.");
            RuleFor(x => x.ZipCode)
                .NotEmpty().WithMessage("'Zip Code' is required.")
                .Matches(@"^\d{5}$").WithMessage("'Zip Code' must be 5 digits.");
        }
    }
}
