using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    // --- Claim 1: Nested condition carries full path ---
    // Claim 3: Nested peer field carries full path ---
    public class NestedBugAddressValidator : ReactiveValidator<NestedBugAddress>
    {
        public NestedBugAddressValidator()
        {
            // Claim 1: WhenField on nested property — condition must carry "Address.City"
            WhenFieldNotEmpty(x => x.City, () =>
            {
                RuleFor(x => x.ConfirmCity).NotEmpty()
                    .WithMessage("Confirm city is required when city is set.");
            });

            // Claim 3: Cross-property comparison — peer field must be "Address.City"
            RuleFor(x => x.ConfirmCity).Equal(x => x.City)
                .WithMessage("Confirm city must match city.")
                .ClientRule(rule => rule.EqualTo(x => x.City));
        }
    }

    public class NestedAddressParentValidator : AbstractValidator<NestedAddressModel>
    {
        public NestedAddressParentValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Address).SetValidator(new NestedBugAddressValidator());
        }
    }

    // --- Claim 2: Parent + child conditions compose ---
    public class ChildSectionValidator : ReactiveValidator<ChildSection>
    {
        public ChildSectionValidator()
        {
            WhenField(x => x.ChildFlag, () =>
            {
                RuleFor(x => x.ChildName).NotEmpty()
                    .WithMessage("Child name required when child flag is checked.");
            });
        }
    }

    public class ParentChildBugValidator : ReactiveValidator<ParentChildModel>
    {
        public ParentChildBugValidator()
        {
            WhenField(x => x.ParentFlag, () =>
            {
                RuleFor(x => x.Child).SetValidator(new ChildSectionValidator());
            });
        }
    }

    // --- Claim 4: Include inside WhenField carries condition ---
    public class SharedEmploymentRulesValidator : AbstractValidator<IncludeModel>
    {
        public SharedEmploymentRulesValidator()
        {
            RuleFor(x => x.JobTitle).NotEmpty()
                .WithMessage("Job title is required.");
            RuleFor(x => x.Department).NotEmpty()
                .WithMessage("Department is required.");
        }
    }

    public class IncludeBugValidator : ReactiveValidator<IncludeModel>
    {
        public IncludeBugValidator()
        {
            WhenField(x => x.IsEmployed, () =>
            {
                Include(new SharedEmploymentRulesValidator());
            });
        }
    }
}
