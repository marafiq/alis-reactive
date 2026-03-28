using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ArchitectureValidator : ReactiveValidator<ArchitectureModel>
    {
        public ArchitectureValidator()
        {
            // Section 6: required fields
            RuleFor(x => x.NativeRequired).NotEmpty().WithMessage("Required");
            RuleFor(x => x.FusionRequired).NotEmpty().WithMessage("Required");

            // Section 7: password + confirm (equalTo)
            RuleFor(x => x.Password).NotEmpty().WithMessage("Required");
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Required")
                .Equal(x => x.Password).WithMessage("Must match");
        }
    }
}
