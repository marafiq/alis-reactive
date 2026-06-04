using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    // Included nested validators must emit Address.* paths for conditions and peer references.
    public class NestedBugAddressValidator : ReactiveValidator<NestedBugAddress>
    {
        public NestedBugAddressValidator()
        {
            WhenFieldNotEmpty(x => x.City, () =>
            {
                ClientRule(x => x.ConfirmCity)
                    .Required("Confirm city is required when city is set.");
            });

            ClientRule(x => x.ConfirmCity)
                .EqualTo(x => x.City, "Confirm city must match city.");
        }
    }

    public class NestedAddressParentValidator : ReactiveValidator<NestedAddressModel>
    {
        public NestedAddressParentValidator()
        {
            ClientRule(x => x.Name)
                .Required("Name is required.");
            ClientRule(x => x.Address, new NestedBugAddressValidator());
        }
    }

    public class ChildSectionValidator : ReactiveValidator<ChildSection>
    {
        public ChildSectionValidator()
        {
            WhenField(x => x.ChildFlag, () =>
            {
                ClientRule(x => x.ChildName)
                    .Required("Child name required when child flag is checked.");
            });
        }
    }

    // Parent and child conditions must compose instead of letting the child rule fire alone.
    public class ParentChildBugValidator : ReactiveValidator<ParentChildModel>
    {
        public ParentChildBugValidator()
        {
            WhenField(x => x.ParentFlag, () =>
            {
                ClientRule(x => x.Child, new ChildSectionValidator());
            });
        }
    }

    public class SharedEmploymentRulesValidator : ReactiveValidator<IncludeModel>
    {
        public SharedEmploymentRulesValidator()
        {
            ClientRule(x => x.JobTitle)
                .Required("Job title is required.");
            ClientRule(x => x.Department)
                .Required("Department is required.");
        }
    }

    // Rules imported inside WhenField must inherit the outer condition.
    public class IncludeBugValidator : ReactiveValidator<IncludeModel>
    {
        public IncludeBugValidator()
        {
            WhenField(x => x.IsEmployed, () =>
            {
                ClientRulesFrom(new SharedEmploymentRulesValidator());
            });
        }
    }
}
