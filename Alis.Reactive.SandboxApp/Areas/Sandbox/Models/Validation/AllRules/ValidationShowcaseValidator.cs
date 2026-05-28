using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ValidationShowcaseValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ValidationShowcaseValidator()
        {
            ClientRule(x => x.AllRules, new AllRulesSectionValidator());
            ClientRule(x => x.Server, new BasicSectionValidator());
            ClientRule(x => x.Live, new BasicSectionValidator());
            ClientRule(x => x.Db, new BasicSectionValidator());
            ClientRule(x => x.Combined, new CombinedSectionValidator());
            ClientRule(x => x.Hidden, new HiddenFieldsSectionValidator());
            ClientRule(x => x.Conditional, new ConditionalSectionValidator());
            ClientRule(x => x.Nested, new NestedSectionValidator());
            ClientRuleEach(x => x.Lines)
                .SetValidator(new ValidationOrderLineValidator());
        }
    }

    public class BasicSectionValidator : ReactiveValidator<BasicSection>
    {
        public BasicSectionValidator()
        {
            ClientRule(x => x.Name)
                .Required("Name is required.")
                .MaxLength(100, "Name must be at most 100 characters.");

            ClientRule(x => x.Email)
                .Required("Email is required.")
                .Email("Email must be a valid email address.");
        }
    }

    public class AllRulesSectionValidator : ReactiveValidator<AllRulesSection>
    {
        public AllRulesSectionValidator()
        {
            ClientRulesFrom(new BasicSectionValidator());

            ClientRule(x => x.Age)
                .Range(0, 120, "Age must be between 0 and 120.");

            ClientRule(x => x.Phone)
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match format 123-456-7890.");

            ClientRule(x => x.Salary)
                .GreaterThanOrEqualTo(0m, "Salary must be at least 0.")
                .LessThanOrEqualTo(500000m, "Salary must be at most 500,000.");

            ClientRule(x => x.Password)
                .MinLength(8, "Password must be at least 8 characters.");
        }
    }

    public class CombinedSectionValidator : ReactiveValidator<CombinedSection>
    {
        public CombinedSectionValidator()
        {
            ClientRulesFrom(new BasicSectionValidator());

            ClientRule(x => x.Age)
                .Range(0, 120, "Age must be between 0 and 120.");

            ClientRule(x => x.Phone)
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match format 123-456-7890.");
        }
    }

    public class HiddenFieldsSectionValidator : ReactiveValidator<HiddenFieldsSection>
    {
        public HiddenFieldsSectionValidator()
        {
            ClientRule(x => x.Name)
                .Required("Name is required.")
                .MaxLength(100, "Name must be at most 100 characters.");

            ClientRule(x => x.Phone)
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match format 123-456-7890.");

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
                ClientRule(x => x.JobTitle)
                    .Required("Job title is required when employed.");
            });
        }
    }

    public class NestedSectionValidator : ReactiveValidator<NestedSection>
    {
        public NestedSectionValidator()
        {
            ClientRule(x => x.Address!, new ValidationAddressValidator());
            ClientRule(x => x.Delivery!, new DeliveryNoteValidator());
        }
    }

    public class DeliveryNoteValidator : ReactiveValidator<DeliveryNote>
    {
        public DeliveryNoteValidator()
        {
            ClientRule(x => x.Instructions)
                .Required("Delivery instructions are required.");

            ClientRule(x => x.ContactPhone)
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match format 123-456-7890.");
        }
    }

    public class ValidationAddressValidator : ReactiveValidator<ValidationAddress>
    {
        public ValidationAddressValidator()
        {
            ClientRule(x => x.Street)
                .Required("Street is required.");

            ClientRule(x => x.City)
                .Required("City is required.");

            ClientRule(x => x.ZipCode)
                .MinLength(5, "Zip code must be at least 5 characters.");
        }
    }

    public class ValidationOrderLineValidator : ReactiveValidator<ValidationOrderLine>
    {
        public ValidationOrderLineValidator()
        {
            ClientRule(x => x.Sku)
                .Required("Line SKU is required.");
        }
    }

    // ── Form-scoped validators (one per form, validates only that form's fields) ──

    public class AllRulesFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public AllRulesFormValidator()
        {
            ClientRule(x => x.AllRules, new AllRulesSectionValidator());
        }
    }

    public class ServerFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ServerFormValidator()
        {
            ClientRule(x => x.Server, new BasicSectionValidator());
        }
    }

    public class LiveFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public LiveFormValidator()
        {
            ClientRule(x => x.Live, new BasicSectionValidator());
        }
    }

    public class CombinedFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public CombinedFormValidator()
        {
            ClientRule(x => x.Combined, new CombinedSectionValidator());
        }
    }

    public class HiddenFieldsFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public HiddenFieldsFormValidator()
        {
            ClientRule(x => x.Hidden, new HiddenFieldsSectionValidator());
        }
    }

    public class ConditionalFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ConditionalFormValidator()
        {
            ClientRule(x => x.Conditional, new ConditionalSectionValidator());
        }
    }

    public class DbFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public DbFormValidator()
        {
            ClientRule(x => x.Db, new BasicSectionValidator());
        }
    }

    public class NestedAddressFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public NestedAddressFormValidator()
        {
            ClientRule(x => x.Nested!, new NestedSectionValidator());
        }
    }

    public class OrderLinesFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public OrderLinesFormValidator()
        {
            ClientRuleEach(x => x.Lines)
                .SetValidator(new ValidationOrderLineValidator());
        }
    }
}
