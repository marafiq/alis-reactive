using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ValidationShowcaseValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ValidationShowcaseValidator()
        {
            RuleFor(x => x.AllRules).SetValidator(new AllRulesSectionValidator());
            ClientRule(x => x.AllRules, new AllRulesSectionValidator());
            RuleFor(x => x.Server).SetValidator(new BasicSectionValidator());
            ClientRule(x => x.Server, new BasicSectionValidator());
            RuleFor(x => x.Live).SetValidator(new BasicSectionValidator());
            ClientRule(x => x.Live, new BasicSectionValidator());
            RuleFor(x => x.Db).SetValidator(new BasicSectionValidator());
            ClientRule(x => x.Db, new BasicSectionValidator());
            RuleFor(x => x.Combined).SetValidator(new CombinedSectionValidator());
            ClientRule(x => x.Combined, new CombinedSectionValidator());
            RuleFor(x => x.Hidden).SetValidator(new HiddenFieldsSectionValidator());
            ClientRule(x => x.Hidden, new HiddenFieldsSectionValidator());
            RuleFor(x => x.Conditional).SetValidator(new ConditionalSectionValidator());
            ClientRule(x => x.Conditional, new ConditionalSectionValidator());
            RuleFor(x => x.Nested).SetValidator(new NestedSectionValidator());
            ClientRule(x => x.Nested, new NestedSectionValidator());
        }
    }

    public class BasicSectionValidator : ReactiveValidator<BasicSection>
    {
        public BasicSectionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");
            ClientRule(x => x.Name)
                .Required("Name is required.")
                .MaxLength(100, "Name must be at most 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
            ClientRule(x => x.Email)
                .Required("Email is required.")
                .Email("Email must be a valid email address.");
        }
    }

    public class AllRulesSectionValidator : ReactiveValidator<AllRulesSection>
    {
        public AllRulesSectionValidator()
        {
            Include(new BasicSectionValidator());
            ClientRulesFrom(new BasicSectionValidator());

            RuleFor(x => x.Age)
                .InclusiveBetween(0, 120).WithMessage("Age must be between 0 and 120.");
            ClientRule(x => x.Age)
                .Range(0, 120, "Age must be between 0 and 120.");

            RuleFor(x => x.Phone)
                .Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");
            ClientRule(x => x.Phone)
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match format 123-456-7890.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0m).WithMessage("Salary must be at least 0.")
                .LessThanOrEqualTo(500000m).WithMessage("Salary must be at most 500,000.");
            ClientRule(x => x.Salary)
                .GreaterThanOrEqualTo(0m, "Salary must be at least 0.")
                .LessThanOrEqualTo(500000m, "Salary must be at most 500,000.");

            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
            ClientRule(x => x.Password)
                .MinLength(8, "Password must be at least 8 characters.");
        }
    }

    public class CombinedSectionValidator : ReactiveValidator<CombinedSection>
    {
        public CombinedSectionValidator()
        {
            Include(new BasicSectionValidator());
            ClientRulesFrom(new BasicSectionValidator());

            RuleFor(x => x.Age)
                .InclusiveBetween(0, 120).WithMessage("Age must be between 0 and 120.");
            ClientRule(x => x.Age)
                .Range(0, 120, "Age must be between 0 and 120.");

            RuleFor(x => x.Phone)
                .Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");
            ClientRule(x => x.Phone)
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match format 123-456-7890.");
        }
    }

    public class HiddenFieldsSectionValidator : ReactiveValidator<HiddenFieldsSection>
    {
        public HiddenFieldsSectionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");
            ClientRule(x => x.Name)
                .Required("Name is required.")
                .MaxLength(100, "Name must be at most 100 characters.");

            RuleFor(x => x.Phone)
                .Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");
            ClientRule(x => x.Phone)
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match format 123-456-7890.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0m).WithMessage("Salary must be at least 0.")
                .LessThanOrEqualTo(500000m).WithMessage("Salary must be at most 500,000.");
            ClientRule(x => x.Salary)
                .GreaterThanOrEqualTo(0m, "Salary must be at least 0.")
                .LessThanOrEqualTo(500000m, "Salary must be at most 500,000.");
        }
    }

    public class ConditionalSectionValidator : ReactiveValidator<ConditionalSection>
    {
        public ConditionalSectionValidator()
        {
            WhenField(x => x.IsEmployed, () =>
            {
                RuleFor(x => x.JobTitle)
                    .NotEmpty().WithMessage("Job title is required when employed.");
                ClientRule(x => x.JobTitle)
                    .Required("Job title is required when employed.");
            });
        }
    }

    public class NestedSectionValidator : ReactiveValidator<NestedSection>
    {
        public NestedSectionValidator()
        {
            RuleFor(x => x.Address!).SetValidator(new ValidationAddressValidator());
            ClientRule(x => x.Address!, new ValidationAddressValidator());
            RuleFor(x => x.Delivery!).SetValidator(new DeliveryNoteValidator());
            ClientRule(x => x.Delivery!, new DeliveryNoteValidator());
        }
    }

    public class DeliveryNoteValidator : ReactiveValidator<DeliveryNote>
    {
        public DeliveryNoteValidator()
        {
            RuleFor(x => x.Instructions)
                .NotEmpty().WithMessage("Delivery instructions are required.");
            ClientRule(x => x.Instructions)
                .Required("Delivery instructions are required.");

            RuleFor(x => x.ContactPhone)
                .Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");
            ClientRule(x => x.ContactPhone)
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match format 123-456-7890.");
        }
    }

    public class ValidationAddressValidator : ReactiveValidator<ValidationAddress>
    {
        public ValidationAddressValidator()
        {
            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required.");
            ClientRule(x => x.Street)
                .Required("Street is required.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.");
            ClientRule(x => x.City)
                .Required("City is required.");

            RuleFor(x => x.ZipCode)
                .MinimumLength(5).WithMessage("Zip code must be at least 5 characters.");
            ClientRule(x => x.ZipCode)
                .MinLength(5, "Zip code must be at least 5 characters.");
        }
    }

    // ── Form-scoped validators (one per form, validates only that form's fields) ──

    public class AllRulesFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public AllRulesFormValidator()
        {
            RuleFor(x => x.AllRules).SetValidator(new AllRulesSectionValidator());
            ClientRule(x => x.AllRules, new AllRulesSectionValidator());
        }
    }

    public class ServerFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ServerFormValidator()
        {
            RuleFor(x => x.Server).SetValidator(new BasicSectionValidator());
            ClientRule(x => x.Server, new BasicSectionValidator());
        }
    }

    public class LiveFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public LiveFormValidator()
        {
            RuleFor(x => x.Live).SetValidator(new BasicSectionValidator());
            ClientRule(x => x.Live, new BasicSectionValidator());
        }
    }

    public class CombinedFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public CombinedFormValidator()
        {
            RuleFor(x => x.Combined).SetValidator(new CombinedSectionValidator());
            ClientRule(x => x.Combined, new CombinedSectionValidator());
        }
    }

    public class HiddenFieldsFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public HiddenFieldsFormValidator()
        {
            RuleFor(x => x.Hidden).SetValidator(new HiddenFieldsSectionValidator());
            ClientRule(x => x.Hidden, new HiddenFieldsSectionValidator());
        }
    }

    public class ConditionalFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ConditionalFormValidator()
        {
            RuleFor(x => x.Conditional).SetValidator(new ConditionalSectionValidator());
            ClientRule(x => x.Conditional, new ConditionalSectionValidator());
        }
    }

    public class DbFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public DbFormValidator()
        {
            RuleFor(x => x.Db).SetValidator(new BasicSectionValidator());
            ClientRule(x => x.Db, new BasicSectionValidator());
        }
    }

    public class NestedAddressFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public NestedAddressFormValidator()
        {
            RuleFor(x => x.Nested!).SetValidator(new NestedSectionValidator());
            ClientRule(x => x.Nested!, new NestedSectionValidator());
        }
    }
}
