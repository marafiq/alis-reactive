using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The family contacts page: the grid holds the contacts, so the page has no fields of its own.</summary>
    public class FamilyContactsViewModel
    {
        /// <summary>Each resident's family contact, as the front desk has it on file.</summary>
        public List<FamilyContact> Contacts { get; set; } = new();
    }

    /// <summary>The family member the front desk calls and writes to for a resident.</summary>
    public class FamilyContact
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";

        /// <summary>The family's page on the community portal.</summary>
        public string PortalLink { get; set; } = "";
        public int VisitsPerWeek { get; set; }
    }

    public class FamilyContactsSaveRequest
    {
        public FusionGridBatchChanges<FamilyContact>? BatchChanges { get; set; }
    }

    /// <summary>The contacts after a save, and what the save did.</summary>
    public class FamilyContactsSaved
    {
        public List<FamilyContact> Contacts { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>
    /// The front desk's rules for a family contact. The grid's cells check the ones they can, as the
    /// server checks them when the edits are saved.
    /// </summary>
    public class FamilyContactValidator : ReactiveValidator<FamilyContact>
    {
        public FamilyContactValidator()
        {
            RuleFor(c => c.ContactName)
                .NotEmpty().WithMessage("Name the family contact.")
                .MinimumLength(2).WithMessage("Write the contact's full name.")
                .MaximumLength(40).WithMessage("Keep the contact's name under 40 characters.");

            RuleFor(c => c.Phone).Matches(@"^\d{3}-\d{4}$").WithMessage("Enter the phone as 555-0100.");

            RuleFor(c => c.Email).EmailAddress().WithMessage("Enter an email address the family reads.");

            RuleFor(c => c.PortalLink).Url().WithMessage("The portal link must be a web address.");

            // The front desk books at most two visits a day.
            RuleFor(c => c.VisitsPerWeek)
                .GreaterThanOrEqualTo(0).WithMessage("Visits per week cannot be negative.")
                .LessThanOrEqualTo(14).WithMessage("The front desk books at most 14 visits a week.");
        }
    }
}
