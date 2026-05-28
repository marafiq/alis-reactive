using Alis.Reactive.FluentValidator;

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
                ClientRule(x => x.ConfirmCity)
                    .Required("Confirm city is required when city is set.");
            });

            // Claim 3: Cross-property comparison — peer field must be "Address.City"
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

    // --- Claim 2: Parent + child conditions compose ---
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

    // --- Claim 4: Composed rules inside WhenField carry condition ---
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
