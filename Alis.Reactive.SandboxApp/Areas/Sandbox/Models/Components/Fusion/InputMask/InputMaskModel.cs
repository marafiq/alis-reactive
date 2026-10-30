using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InputMask
{
    /// <summary>
    /// The phone numbers the front desk keeps for a resident's family: the family member's, and the
    /// emergency contact's, which may be the same person's.
    /// </summary>
    public sealed class InputMaskModel
    {
        /// <summary>The family member's phone, as its ten digits.</summary>
        public string? FamilyPhone { get; set; }

        /// <summary>Whether the family member is also the emergency contact.</summary>
        public bool EmergencyIsFamily { get; set; }

        /// <summary>The emergency contact's phone, as its ten digits.</summary>
        public string? EmergencyPhone { get; set; }
    }

    /// <summary>The desk keeps a number only when all ten digits are in.</summary>
    public sealed class FamilyPhoneNumbersValidator : ReactiveValidator<InputMaskModel>
    {
        public FamilyPhoneNumbersValidator()
        {
            RuleFor(x => x.FamilyPhone)
                .NotEmpty().WithMessage("Enter the family member's phone number.")
                .Matches(@"^\d{10}$").WithMessage("Enter all ten digits of the family member's phone number.");
            RuleFor(x => x.EmergencyPhone)
                .NotEmpty().WithMessage("Enter the emergency contact's phone number.")
                .Matches(@"^\d{10}$").WithMessage("Enter all ten digits of the emergency contact's phone number.");
        }
    }

    /// <summary>What the desk is told about the numbers it saved.</summary>
    public sealed class FamilyPhoneNumbersSaved
    {
        /// <summary>The numbers saved, written as the desk dials them.</summary>
        public string Message { get; set; } = string.Empty;
    }
}
