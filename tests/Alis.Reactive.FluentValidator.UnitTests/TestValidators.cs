using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

// --- Required rules ---

public class RequiredValidator : AbstractValidator<TestModel>
{
    public RequiredValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class NotNullValidator : AbstractValidator<TestModel>
{
    public NotNullValidator()
    {
        RuleFor(x => x.Name).NotNull();
    }
}

public class RequiredWithCustomMessageValidator : AbstractValidator<TestModel>
{
    public RequiredWithCustomMessageValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be blank.");
    }
}

// --- Length rules ---

public class MaxLengthValidator : AbstractValidator<TestModel>
{
    public MaxLengthValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100);
    }
}

public class MinLengthValidator : AbstractValidator<TestModel>
{
    public MinLengthValidator()
    {
        RuleFor(x => x.Name).MinimumLength(3);
    }
}

public class BothLengthValidator : AbstractValidator<TestModel>
{
    public BothLengthValidator()
    {
        RuleFor(x => x.Name).MinimumLength(3).MaximumLength(100);
    }
}

// --- Email rule ---

public class EmailValidator : AbstractValidator<TestModel>
{
    public EmailValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
    }
}

public class EmailWithCustomMessageValidator : AbstractValidator<TestModel>
{
    public EmailWithCustomMessageValidator()
    {
        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email format.");
    }
}

// --- Regex rule ---

public class RegexValidator : AbstractValidator<TestModel>
{
    public RegexValidator()
    {
        RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$");
    }
}

// --- Range rule ---

public class RangeValidator : AbstractValidator<TestModel>
{
    public RangeValidator()
    {
        RuleFor(x => x.Age).InclusiveBetween(0, 120);
    }
}

// --- Comparison rules ---

public class MinComparisonValidator : AbstractValidator<TestModel>
{
    public MinComparisonValidator()
    {
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m);
    }
}

public class MaxComparisonValidator : AbstractValidator<TestModel>
{
    public MaxComparisonValidator()
    {
        RuleFor(x => x.Salary).LessThanOrEqualTo(500000m);
    }
}

// --- Nested validators ---

public class TestAddressValidator : AbstractValidator<TestAddress>
{
    public TestAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.ZipCode).MinimumLength(5);
    }
}

public class NestedValidator : AbstractValidator<TestModel>
{
    public NestedValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address!).SetValidator(new TestAddressValidator());
    }
}

public class TestCountryValidator : AbstractValidator<TestCountry>
{
    public TestCountryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(3);
    }
}

public class TestDeepAddressValidator : AbstractValidator<TestDeepAddress>
{
    public TestDeepAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.Country!).SetValidator(new TestCountryValidator());
    }
}

public class DeeplyNestedValidator : AbstractValidator<TestModel>
{
    public DeeplyNestedValidator()
    {
        RuleFor(x => x.DeepAddress!).SetValidator(new TestDeepAddressValidator());
    }
}

// --- Conditional rules (skipped for client) ---

public class ConditionalValidator : AbstractValidator<TestModel>
{
    public ConditionalValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.JobTitle).NotEmpty().When(x => x.IsEmployed);
    }
}

// --- Conditional rules via ReactiveValidator (replaces IConditionalRuleProvider) ---

public class ConditionalProviderValidator : ReactiveValidator<TestModel>
{
    public ConditionalProviderValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        WhenField(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.JobTitle).NotEmpty();
        });
    }
}

// --- ReactiveValidator conditional rules ---

public class ReactiveConditionalValidator : ReactiveValidator<TestModel>
{
    public ReactiveConditionalValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        WhenField(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.JobTitle).NotEmpty();
        });
    }
}

public class ReactiveMultipleRulesValidator : ReactiveValidator<TestModel>
{
    public ReactiveMultipleRulesValidator()
    {
        WhenField(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.JobTitle).NotEmpty().MinimumLength(3);
            RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m);
        });
    }
}

public class ReactiveEqConditionValidator : ReactiveValidator<TestModel>
{
    public ReactiveEqConditionValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        WhenField(x => x.Name, "Admin", () =>
        {
            RuleFor(x => x.Email).EmailAddress();
        });
    }
}

public class ReactiveMixedValidator : ReactiveValidator<TestModel>
{
    public ReactiveMixedValidator()
    {
        // Unconditional
        RuleFor(x => x.Name).NotEmpty();
        // Client-conditional via WhenField
        WhenField(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.JobTitle).NotEmpty();
        });
        // Server-only via .When() — should be skipped by adapter
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m).When(x => x.Age > 18);
    }
}

// --- Empty validator ---

public class EmptyValidator : AbstractValidator<TestModel>
{
    public EmptyValidator() { }
}

// --- Multiple rules per field ---

public class MultipleRulesValidator : AbstractValidator<TestModel>
{
    public MultipleRulesValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
    }
}

// --- EqualTo rules ---

public class EqualToValidator : AbstractValidator<TestModel>
{
    public EqualToValidator()
    {
        RuleFor(x => x.ConfirmEmail).Equal(x => x.Email);
    }
}

public class EqualToWithCustomMessageValidator : AbstractValidator<TestModel>
{
    public EqualToWithCustomMessageValidator()
    {
        RuleFor(x => x.ConfirmEmail).Equal(x => x.Email).WithMessage("Emails must match.");
    }
}

// --- Broken nested validator (for fail-fast test) ---

public class BrokenNestedValidator : AbstractValidator<TestModel>
{
    public BrokenNestedValidator()
    {
        RuleFor(x => x.Address!).SetValidator(new TestAddressValidator());
    }
}

// --- All rule types in one validator ---

public class AllRulesValidator : AbstractValidator<TestModel>
{
    public AllRulesValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$");
        RuleFor(x => x.Age).InclusiveBetween(0, 120);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m).LessThanOrEqualTo(500000m);
    }
}
