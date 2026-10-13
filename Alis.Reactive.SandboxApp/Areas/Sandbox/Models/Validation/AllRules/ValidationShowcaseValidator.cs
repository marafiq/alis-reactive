using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ValidationShowcaseValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ValidationShowcaseValidator()
        {
            RuleFor(x => x.AllRules).SetValidator(new AllRulesSectionValidator());
            RuleFor(x => x.Server).SetValidator(new BasicSectionValidator());
            RuleFor(x => x.Live).SetValidator(new BasicSectionValidator());
            RuleFor(x => x.Db).SetValidator(new BasicSectionValidator());
            RuleFor(x => x.Combined).SetValidator(new CombinedSectionValidator());
            RuleFor(x => x.Hidden).SetValidator(new HiddenFieldsSectionValidator());
            RuleFor(x => x.Conditional).SetValidator(new ConditionalSectionValidator());
            RuleFor(x => x.Nested).SetValidator(new NestedSectionValidator());
            RuleForEach(x => x.Lines).SetValidator(new ValidationOrderLineValidator());
        }
    }

    public class BasicSectionValidator : ReactiveValidator<BasicSection>
    {
        public BasicSectionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
        }
    }

    public class AllRulesSectionValidator : ReactiveValidator<AllRulesSection>
    {
        public AllRulesSectionValidator()
        {
            Include(new BasicSectionValidator());

            RuleFor(x => x.Age).InclusiveBetween(0, 120).WithMessage("Age must be between 0 and 120.");

            RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0m).WithMessage("Salary must be at least 0.")
                .LessThanOrEqualTo(500000m).WithMessage("Salary must be at most 500,000.");

            RuleFor(x => x.Password).MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        }
    }

    public class CombinedSectionValidator : ReactiveValidator<CombinedSection>
    {
        public CombinedSectionValidator()
        {
            Include(new BasicSectionValidator());

            RuleFor(x => x.Age).InclusiveBetween(0, 120).WithMessage("Age must be between 0 and 120.");

            RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");
        }
    }

    public class HiddenFieldsSectionValidator : ReactiveValidator<HiddenFieldsSection>
    {
        public HiddenFieldsSectionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0m).WithMessage("Salary must be at least 0.")
                .LessThanOrEqualTo(500000m).WithMessage("Salary must be at most 500,000.");
        }
    }

    public class ConditionalSectionValidator : ReactiveValidator<ConditionalSection>
    {
        public ConditionalSectionValidator()
        {
            When(x => x.IsEmployed, () =>
            {
                RuleFor(x => x.JobTitle).NotEmpty().WithMessage("Job title is required when employed.");
            });
        }
    }

    public class NestedSectionValidator : ReactiveValidator<NestedSection>
    {
        public NestedSectionValidator()
        {
            RuleFor(x => x.Address!).SetValidator(new ValidationAddressValidator());
            RuleFor(x => x.Delivery!).SetValidator(new DeliveryNoteValidator());
        }
    }

    public class DeliveryNoteValidator : ReactiveValidator<DeliveryNote>
    {
        public DeliveryNoteValidator()
        {
            RuleFor(x => x.Instructions).NotEmpty().WithMessage("Delivery instructions are required.");

            RuleFor(x => x.ContactPhone).Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone must match format 123-456-7890.");
        }
    }

    public class ValidationAddressValidator : ReactiveValidator<ValidationAddress>
    {
        public ValidationAddressValidator()
        {
            RuleFor(x => x.Street).NotEmpty().WithMessage("Street is required.");

            RuleFor(x => x.City).NotEmpty().WithMessage("City is required.");

            RuleFor(x => x.ZipCode).MinimumLength(5).WithMessage("Zip code must be at least 5 characters.");
        }
    }

    public class ValidationOrderLineValidator : ReactiveValidator<ValidationOrderLine>
    {
        public ValidationOrderLineValidator()
        {
            RuleFor(x => x.Sku).NotEmpty().WithMessage("Line SKU is required.");

            RuleFor(x => x.ConfirmSku).Equal(x => x.Sku).WithMessage("Line SKU confirmation must match.");

            When(x => x.GiftWrapped, () =>
            {
                RuleFor(x => x.GiftNote).NotEmpty().WithMessage("Gift note is required when gift wrapped.");
            });

            RuleForEach(x => x.Deliveries).SetValidator(new ValidationLineDeliveryValidator());
        }
    }

    public class ValidationLineDeliveryValidator : ReactiveValidator<ValidationLineDelivery>
    {
        public ValidationLineDeliveryValidator()
        {
            RuleFor(x => x.LocationCode).NotEmpty().WithMessage("Delivery location is required.");

            When(x => x.RequiresAccessCode, () =>
            {
                RuleFor(x => x.AccessCode).NotEmpty().WithMessage("Access code is required when delivery requires it.");
            });
        }
    }

    public class AllRulesFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public AllRulesFormValidator()
        {
            RuleFor(x => x.AllRules).SetValidator(new AllRulesSectionValidator());
        }
    }

    public class ServerFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ServerFormValidator()
        {
            RuleFor(x => x.Server).SetValidator(new BasicSectionValidator());
        }
    }

    public class LiveFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public LiveFormValidator()
        {
            RuleFor(x => x.Live).SetValidator(new BasicSectionValidator());
        }
    }

    public class CombinedFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public CombinedFormValidator()
        {
            RuleFor(x => x.Combined).SetValidator(new CombinedSectionValidator());
        }
    }

    public class HiddenFieldsFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public HiddenFieldsFormValidator()
        {
            RuleFor(x => x.Hidden).SetValidator(new HiddenFieldsSectionValidator());
        }
    }

    public class ConditionalFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public ConditionalFormValidator()
        {
            RuleFor(x => x.Conditional).SetValidator(new ConditionalSectionValidator());
        }
    }

    public class DbFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public DbFormValidator()
        {
            RuleFor(x => x.Db).SetValidator(new BasicSectionValidator());
        }
    }

    public class NestedAddressFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public NestedAddressFormValidator()
        {
            RuleFor(x => x.Nested!).SetValidator(new NestedSectionValidator());
        }
    }

    public class OrderLinesFormValidator : ReactiveValidator<ValidationShowcaseModel>
    {
        public OrderLinesFormValidator()
        {
            RuleForEach(x => x.Lines).SetValidator(new ValidationOrderLineValidator());
        }
    }
}
