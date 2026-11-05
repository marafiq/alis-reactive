
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InputMask
{
    public sealed class InputMaskModel
    {
        public string? FamilyPhone { get; set; }

        public bool EmergencyIsFamily { get; set; }

        public string? EmergencyPhone { get; set; }
    }

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

    public sealed class FamilyPhoneNumbersSaved
    {
        public string Message { get; set; } = string.Empty;
    }
}
