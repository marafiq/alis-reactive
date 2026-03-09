using FluentValidation;
using Alis.Reactive.Validation;

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

// --- Conditional rules via IConditionalRuleProvider ---

public class ConditionalProviderValidator : AbstractValidator<TestModel>, IConditionalRuleProvider
{
    public ConditionalProviderValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        // The .When() rule below is skipped by the adapter.
        // The IConditionalRuleProvider supplies the explicit conditional rule instead.
        RuleFor(x => x.JobTitle).NotEmpty().When(x => x.IsEmployed);
    }

    public IReadOnlyList<ConditionalRuleMetadata> GetConditionalRules()
    {
        return new List<ConditionalRuleMetadata>
        {
            new ConditionalRuleMetadata(
                "JobTitle",
                "required",
                "'Job Title' is required when employed.",
                new ValidationCondition("IsEmployed", "truthy"))
        };
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
