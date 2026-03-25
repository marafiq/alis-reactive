using FluentValidation;
using Alis.Reactive.FluentValidator.Validators;

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

public class StrictGreaterThanValidator : AbstractValidator<TestModel>
{
    public StrictGreaterThanValidator()
    {
        RuleFor(x => x.Salary).GreaterThan(0m);
    }
}

public class StrictLessThanValidator : AbstractValidator<TestModel>
{
    public StrictLessThanValidator()
    {
        RuleFor(x => x.Salary).LessThan(1000000m);
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

// --- ReactiveValidator falsy/neq conditions (WhenFieldNot) ---

public class ReactiveFalsyConditionValidator : ReactiveValidator<TestModel>
{
    public ReactiveFalsyConditionValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        WhenFieldNot(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.JobTitle).NotEmpty().WithMessage("Explain why not employed");
        });
    }
}

public class ReactiveNeqConditionValidator : ReactiveValidator<TestModel>
{
    public ReactiveNeqConditionValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        WhenFieldNot(x => x.Name, "Independent", () =>
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email required unless independent");
        });
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

// --- Conditional parity: ALL rule types under WhenField ---

public class ConditionalAllRulesValidator : ReactiveValidator<TestModel>
{
    public ConditionalAllRulesValidator()
    {
        WhenField(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name required when employed");
            RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name min 3");
            RuleFor(x => x.Name).MaximumLength(100).WithMessage("Name max 100");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Valid email required");
            RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Phone format");
            RuleFor(x => x.Age).InclusiveBetween(18, 65).WithMessage("Age 18-65");
            RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m).WithMessage("Salary min 0");
            RuleFor(x => x.Salary).LessThanOrEqualTo(500000m).WithMessage("Salary max 500k");
            RuleFor(x => x.Salary).GreaterThan(0m).WithMessage("Salary gt 0");
            RuleFor(x => x.Salary).LessThan(1000000m).WithMessage("Salary lt 1M");
            RuleFor(x => x.ConfirmEmail).Equal(x => x.Email).WithMessage("Emails must match");
        });
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

// --- Full coverage validator (all 18 rule types) ---

public class FullCoverageValidator : AbstractValidator<FullCoverageModel>
{
    public FullCoverageValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name).MinimumLength(3);
        RuleFor(x => x.Name).MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$");
        RuleFor(x => x.CreditCardNumber).CreditCard();
        RuleFor(x => x.Age).InclusiveBetween(0, 120);
        RuleFor(x => x.Score).IsExclusiveBetween(0m, 100m);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Salary).LessThanOrEqualTo(500000m);
        RuleFor(x => x.MonthlyRate).GreaterThan(0m);
        RuleFor(x => x.MonthlyRate).LessThan(1000000m);
        RuleFor(x => x.ConfirmEmail).Equal(x => x.Email);
        RuleFor(x => x.AlternateEmail).NotEqual(x => x.Email);
        RuleFor(x => x.Status).NotEqual("deleted");
        RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020, 1, 1));
        RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate);
        RuleFor(x => x.Nickname).IsEmpty();
    }
}

public class FullCoverageConditionalValidator : ReactiveValidator<FullCoverageModel>
{
    public FullCoverageConditionalValidator()
    {
        WhenField(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name).MinimumLength(3);
            RuleFor(x => x.Name).MaximumLength(100);
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$");
            RuleFor(x => x.CreditCardNumber).CreditCard();
            RuleFor(x => x.Age).InclusiveBetween(0, 120);
            RuleFor(x => x.Score).IsExclusiveBetween(0m, 100m);
            RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m);
            RuleFor(x => x.Salary).LessThanOrEqualTo(500000m);
            RuleFor(x => x.MonthlyRate).GreaterThan(0m);
            RuleFor(x => x.MonthlyRate).LessThan(1000000m);
            RuleFor(x => x.ConfirmEmail).Equal(x => x.Email);
            RuleFor(x => x.AlternateEmail).NotEqual(x => x.Email);
            RuleFor(x => x.Status).NotEqual("deleted");
            RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020, 1, 1));
            RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate);
            RuleFor(x => x.Nickname).IsEmpty();
        });
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

// --- DateTime condition validators (WhenField<DateTime> → Unix ms) ---

public class DateTimeConditionValidator : ReactiveValidator<FullCoverageModel>
{
    public DateTimeConditionValidator()
    {
        // eq: Name required when AdmissionDate equals a specific date
        WhenField(x => x.AdmissionDate, new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), () =>
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name required for July 1 admissions");
        });
    }
}

public class DateTimeNeqConditionValidator : ReactiveValidator<FullCoverageModel>
{
    public DateTimeNeqConditionValidator()
    {
        // neq: Score required when AdmissionDate is NOT a specific date
        WhenFieldNot(x => x.AdmissionDate, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), () =>
        {
            RuleFor(x => x.Score).GreaterThan(0m).WithMessage("Score required for non-Jan-1 dates");
        });
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
