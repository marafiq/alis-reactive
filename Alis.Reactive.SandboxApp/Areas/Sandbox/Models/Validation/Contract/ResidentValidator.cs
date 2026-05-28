using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentValidator : ReactiveValidator<ResidentModel>
    {
        public ResidentValidator()
        {
            // Unconditional
            ClientRule(x => x.Name)
                .Required("'Name' is required.")
                .MinLength(2, "'Name' must have a minimum length of 2.");
            ClientRule(x => x.Email)
                .Required("'Email' is required.")
                .Email("'Email' must be a valid email address.");
            ClientRule(x => x.CareLevel)
                .Required("'Care Level' is required.");

            ClientRule(x => x.ConfirmEmail)
                .Required("'Confirm Email' is required.")
                .EqualTo(x => x.Email, "'Confirm Email' must match 'Email'.");

            WhenField(x => x.IsVeteran, () =>
            {
                ClientRule(x => x.VeteranId)
                    .Required("'Veteran ID' is required when veteran.");
            });

            WhenField(x => x.CareLevel, "Memory Care", () =>
            {
                ClientRule(x => x.MemoryAssessmentScore)
                    .Required("'Memory Assessment' is required for Memory Care.");
            });

            WhenFieldNot(x => x.CareLevel, "Independent", () =>
            {
                ClientRule(x => x.PhysicianName)
                    .Required("'Physician' is required unless Independent.");
            });

            WhenField(x => x.HasEmergencyContact, () =>
            {
                ClientRule(x => x.EmergencyName)
                    .Required("'Emergency Name' is required.");
                ClientRule(x => x.EmergencyPhone)
                    .Required("'Emergency Phone' is required.");
            });

            WhenFieldNot(x => x.HasEmergencyContact, () =>
            {
                ClientRule(x => x.ReasonForNoContact)
                    .Required("'Reason' is required when no emergency contact.");
            });

            ClientRule(x => x.Address, new ResidentAddressValidator());
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
            ClientRule(x => x.Name)
                .Required("'Name' is required.")
                .MinLength(2, "'Name' must have a minimum length of 2.");
            ClientRule(x => x.Email)
                .Required("'Email' is required.")
                .Email("'Email' must be a valid email address.");
            ClientRule(x => x.CareLevel)
                .Required("'Care Level' is required.");

            ClientRule(x => x.ConfirmEmail)
                .Required("'Confirm Email' is required.")
                .EqualTo(x => x.Email, "'Confirm Email' must match 'Email'.");

            WhenField(x => x.IsVeteran, () =>
            {
                ClientRule(x => x.VeteranId)
                    .Required("'Veteran ID' is required when veteran.");
            });

            WhenField(x => x.HasEmergencyContact, () =>
            {
                ClientRule(x => x.EmergencyName)
                    .Required("'Emergency Name' is required.");
                ClientRule(x => x.EmergencyPhone)
                    .Required("'Emergency Phone' is required.");
            });

            WhenFieldNot(x => x.HasEmergencyContact, () =>
            {
                ClientRule(x => x.ReasonForNoContact)
                    .Required("'Reason' is required when no emergency contact.");
            });

            ClientRule(x => x.Address, new ResidentAddressValidator());
        }
    }

    /// <summary>
    /// Scoped validator for AjaxPartial — parent fields + address.
    /// Address fields are unenriched at boot (partial not loaded yet) → skipped.
    /// After partial loads and merges components, enrichment activates address fields.
    /// </summary>
    public class AjaxPartialValidator : ReactiveValidator<ResidentModel>
    {
        public AjaxPartialValidator()
        {
            ClientRule(x => x.Name)
                .Required("'Name' is required.")
                .MinLength(2, "'Name' must have a minimum length of 2.");
            ClientRule(x => x.Email)
                .Required("'Email' is required.")
                .Email("'Email' must be a valid email address.");

            ClientRule(x => x.ConfirmEmail)
                .Required("'Confirm Email' is required.")
                .EqualTo(x => x.Email, "'Confirm Email' must match 'Email'.");

            // Address rules conditional on user selecting "Custom Address".
            // When Facility Address or nothing selected → rules skipped.
            // When Custom Address selected but partial not loaded → unenriched → summary.
            // When Custom Address selected and partial loaded → enriched → inline.
            WhenField(x => x.AddressType, "Custom Address", () =>
            {
                ClientRule(x => x.Address, new ResidentAddressValidator());
            });
        }
    }

    public class ResidentAddressValidator : ReactiveValidator<ResidentAddress>
    {
        public ResidentAddressValidator()
        {
            ClientRule(x => x.Street)
                .Required("'Street' is required.");
            ClientRule(x => x.City)
                .Required("'City' is required.");
            ClientRule(x => x.ZipCode)
                .Required("'Zip Code' is required.")
                .Regex(@"^\d{5}$", "'Zip Code' must be 5 digits.");
        }
    }
}
