using FluentValidation;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.AllRules;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.Contract
{
    public class ValidationMergeHarnessValidator : AbstractValidator<ValidationMergeHarnessModel>
    {
        public ValidationMergeHarnessValidator()
        {
            RuleFor(x => x.Root).SetValidator(new ValidationMergeRootValidator());
            RuleFor(x => x.Nested.Address!).SetValidator(new ValidationAddressValidator());
            RuleFor(x => x.Nested.Delivery!).SetValidator(new DeliveryNoteValidator());
        }
    }

    public class ValidationIsolationValidator : AbstractValidator<ValidationMergeHarnessModel>
    {
        public ValidationIsolationValidator()
        {
            RuleFor(x => x.Root).SetValidator(new ValidationIsolationRootValidator());
            RuleFor(x => x.Nested.Address!).SetValidator(new ValidationAddressValidator());
        }
    }

    public class ValidationMergeRootValidator : AbstractValidator<ValidationMergeRootSection>
    {
        public ValidationMergeRootValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
        }
    }

    public class ValidationIsolationRootValidator : AbstractValidator<ValidationMergeRootSection>
    {
        public ValidationIsolationRootValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
        }
    }
}
