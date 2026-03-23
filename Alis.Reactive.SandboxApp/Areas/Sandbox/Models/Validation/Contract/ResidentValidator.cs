using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.Contract
{
    public class ResidentValidator : ReactiveValidator<ResidentModel>
    {
        public ResidentValidator()
        {
            // Unconditional
            RuleFor(x => x.Name).NotEmpty().WithMessage("'Name' is required.")
                .MinimumLength(2).WithMessage("'Name' must have a minimum length of 2.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' must be a valid email address.");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");

            // equalTo
            RuleFor(x => x.ConfirmEmail).NotEmpty().WithMessage("'Confirm Email' is required.")
                .Equal(x => x.Email).WithMessage("'Confirm Email' must match 'Email'.");

            // truthy
            WhenField(x => x.IsVeteran, () =>
            {
                RuleFor(x => x.VeteranId).NotEmpty().WithMessage("'Veteran ID' is required when veteran.");
            });

            // eq
            WhenField(x => x.CareLevel, "Memory Care", () =>
            {
                RuleFor(x => x.MemoryAssessmentScore).NotEmpty().WithMessage("'Memory Assessment' is required for Memory Care.");
            });

            // neq
            WhenFieldNot(x => x.CareLevel, "Independent", () =>
            {
                RuleFor(x => x.PhysicianName).NotEmpty().WithMessage("'Physician' is required unless Independent.");
            });

            // truthy
            WhenField(x => x.HasEmergencyContact, () =>
            {
                RuleFor(x => x.EmergencyName).NotEmpty().WithMessage("'Emergency Name' is required.");
                RuleFor(x => x.EmergencyPhone).NotEmpty().WithMessage("'Emergency Phone' is required.");
            });

            // falsy
            WhenFieldNot(x => x.HasEmergencyContact, () =>
            {
                RuleFor(x => x.ReasonForNoContact).NotEmpty().WithMessage("'Reason' is required when no emergency contact.");
            });

            // Address (unconditional)
            RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
        }
    }

    /// <summary>
    /// Scoped validator for the ServerPartial page — excludes MemoryAssessmentScore
    /// and PhysicianName which are not rendered on that page.
    /// </summary>
    public class ServerPartialValidator : ReactiveValidator<ResidentModel>
    {
        public ServerPartialValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("'Name' is required.")
                .MinimumLength(2).WithMessage("'Name' must have a minimum length of 2.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' must be a valid email address.");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");

            RuleFor(x => x.ConfirmEmail).NotEmpty().WithMessage("'Confirm Email' is required.")
                .Equal(x => x.Email).WithMessage("'Confirm Email' must match 'Email'.");

            WhenField(x => x.IsVeteran, () =>
            {
                RuleFor(x => x.VeteranId).NotEmpty().WithMessage("'Veteran ID' is required when veteran.");
            });

            WhenField(x => x.HasEmergencyContact, () =>
            {
                RuleFor(x => x.EmergencyName).NotEmpty().WithMessage("'Emergency Name' is required.");
                RuleFor(x => x.EmergencyPhone).NotEmpty().WithMessage("'Emergency Phone' is required.");
            });

            WhenFieldNot(x => x.HasEmergencyContact, () =>
            {
                RuleFor(x => x.ReasonForNoContact).NotEmpty().WithMessage("'Reason' is required when no emergency contact.");
            });

            RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
        }
    }

    /// <summary>
    /// Scoped validator for the AjaxPartial page — only Name, Email, ConfirmEmail + Address.
    /// </summary>
    /// <summary>
    /// Scoped validator for the AjaxPartial page.
    /// Address rules included — they're unenriched before partial loads,
    /// but allRulesConditionallySkipped handles that gracefully.
    /// After partial merges, address fields enrich and validate inline.
    /// </summary>
    /// <summary>
    /// Scoped validator for AjaxPartial — parent fields + address.
    /// Address fields are unenriched at boot (partial not loaded yet) → skipped.
    /// After partial loads and merges components, enrichment activates address fields.
    /// </summary>
    public class AjaxPartialValidator : ReactiveValidator<ResidentModel>
    {
        public AjaxPartialValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("'Name' is required.")
                .MinimumLength(2).WithMessage("'Name' must have a minimum length of 2.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' must be a valid email address.");

            RuleFor(x => x.ConfirmEmail).NotEmpty().WithMessage("'Confirm Email' is required.")
                .Equal(x => x.Email).WithMessage("'Confirm Email' must match 'Email'.");

            // Address rules conditional on user selecting "Custom Address".
            // When Facility Address or nothing selected → rules skipped.
            // When Custom Address selected but partial not loaded → unenriched → summary.
            // When Custom Address selected and partial loaded → enriched → inline.
            WhenField(x => x.AddressType, "Custom Address", () =>
            {
                RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
            });
        }
    }

    public class ResidentAddressValidator : AbstractValidator<ResidentAddress>
    {
        public ResidentAddressValidator()
        {
            RuleFor(x => x.Street).NotEmpty().WithMessage("'Street' is required.");
            RuleFor(x => x.City).NotEmpty().WithMessage("'City' is required.");
            RuleFor(x => x.ZipCode).NotEmpty().WithMessage("'Zip Code' is required.")
                .Matches(@"^\d{5}$").WithMessage("'Zip Code' must be 5 digits.");
        }
    }
}
