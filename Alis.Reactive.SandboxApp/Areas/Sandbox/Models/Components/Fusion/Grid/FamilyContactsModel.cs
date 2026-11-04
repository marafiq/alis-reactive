using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FamilyContactsViewModel
    {
        public List<FamilyContact> Contacts { get; set; } = new();
    }

    public class FamilyContact
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";

        public string PortalLink { get; set; } = "";
        public int VisitsPerWeek { get; set; }
    }

    public class FamilyContactsSaveRequest
    {
        public FusionGridBatchChanges<FamilyContact>? BatchChanges { get; set; }
    }

    public class FamilyContactsSaved
    {
        public List<FamilyContact> Contacts { get; set; } = new();
        public string Summary { get; set; } = "";
    }

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

            RuleFor(c => c.VisitsPerWeek)
                .GreaterThanOrEqualTo(0).WithMessage("Visits per week cannot be negative.")
                .LessThanOrEqualTo(14).WithMessage("The front desk books at most 14 visits a week.");
        }
    }
}
