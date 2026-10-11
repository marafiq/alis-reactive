using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    // Included nested validators must emit Address.* paths for conditions and peer references.
    public class NestedBugAddressValidator : ReactiveValidator<NestedBugAddress>
    {
        public NestedBugAddressValidator()
        {
            When(x => !string.IsNullOrEmpty(x.City), () =>
            {
                RuleFor(x => x.ConfirmCity).NotEmpty().WithMessage("Confirm city is required when city is set.");
            });

            RuleFor(x => x.ConfirmCity).Equal(x => x.City).WithMessage("Confirm city must match city.");
        }
    }

    public class NestedAddressParentValidator : ReactiveValidator<NestedAddressModel>
    {
        public NestedAddressParentValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Address).SetValidator(new NestedBugAddressValidator());
        }
    }

    public class ChildSectionValidator : ReactiveValidator<ChildSection>
    {
        public ChildSectionValidator()
        {
            When(x => x.ChildFlag, () =>
            {
                RuleFor(x => x.ChildName).NotEmpty().WithMessage("Child name required when child flag is checked.");
            });
        }
    }

    // Parent and child conditions must compose instead of letting the child rule fire alone.
    public class ParentChildBugValidator : ReactiveValidator<ParentChildModel>
    {
        public ParentChildBugValidator()
        {
            When(x => x.ParentFlag, () =>
            {
                RuleFor(x => x.Child).SetValidator(new ChildSectionValidator());
            });
        }
    }

    public class SharedEmploymentRulesValidator : ReactiveValidator<IncludeModel>
    {
        public SharedEmploymentRulesValidator()
        {
            RuleFor(x => x.JobTitle).NotEmpty().WithMessage("Job title is required.");
            RuleFor(x => x.Department).NotEmpty().WithMessage("Department is required.");
        }
    }

    // Rules included inside When must inherit the outer condition.
    public class IncludeBugValidator : ReactiveValidator<IncludeModel>
    {
        public IncludeBugValidator()
        {
            When(x => x.IsEmployed, () =>
            {
                Include(new SharedEmploymentRulesValidator());
            });
        }
    }
}
