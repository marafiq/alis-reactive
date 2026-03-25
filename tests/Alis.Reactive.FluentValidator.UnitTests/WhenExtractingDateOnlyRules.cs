using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

/// <summary>
/// Bug F2: SerializeDateConstraint does not handle DateOnly — returns the raw DateOnly object
/// instead of a "yyyy-MM-dd" string. InferCoerceAs correctly maps DateOnly → "date", so the
/// branch `if (coerceAs == "date" && constraint != null) constraint = SerializeDateConstraint(constraint)`
/// IS reached, but SerializeDateConstraint falls through to `return value` because it only handles
/// DateTime and DateTimeOffset. The constraint is serialised as a raw CLR object, not a string.
/// </summary>
[TestFixture]
public class WhenExtractingDateOnlyRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    // --- Test model with DateOnly property ---

    public class DateOnlyModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }

    // --- Validators ---

    public class DateOnlyGreaterThanOrEqualToValidator : AbstractValidator<DateOnlyModel>
    {
        public DateOnlyGreaterThanOrEqualToValidator()
        {
            RuleFor(x => x.StartDate).GreaterThanOrEqualTo(new DateOnly(2026, 1, 1));
        }
    }

    public class DateOnlyLessThanOrEqualToValidator : AbstractValidator<DateOnlyModel>
    {
        public DateOnlyLessThanOrEqualToValidator()
        {
            RuleFor(x => x.StartDate).LessThanOrEqualTo(new DateOnly(2026, 12, 31));
        }
    }

    public class DateOnlyRangeValidator : AbstractValidator<DateOnlyModel>
    {
        public DateOnlyRangeValidator()
        {
            RuleFor(x => x.StartDate).InclusiveBetween(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
        }
    }

    // --- Tests ---

    [Test]
    public void DateOnly_GreaterThanOrEqualTo_constraint_is_serialized_as_yyyy_MM_dd_string()
    {
        // Bug: SerializeDateConstraint has no DateOnly branch — falls through to `return value`,
        // returning the raw DateOnly struct. The constraint is a DateOnly, not a string.
        var desc = _adapter.ExtractRules(typeof(DateOnlyGreaterThanOrEqualToValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var rule = desc!.Fields[0].Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("min"));
        Assert.That(rule.CoerceAs, Is.EqualTo("date"),
            "InferCoerceAs should map DateOnly to 'date'");

        // THIS ASSERTION FAILS TODAY — constraint is a raw DateOnly object, not a string
        Assert.That(rule.Constraint, Is.InstanceOf<string>(),
            "DateOnly constraint must be serialized as a string, not a raw DateOnly object");
        Assert.That(rule.Constraint, Is.EqualTo("2026-01-01"),
            "DateOnly(2026, 1, 1) must serialize to 'yyyy-MM-dd' format");
    }

    [Test]
    public void DateOnly_LessThanOrEqualTo_constraint_is_serialized_as_yyyy_MM_dd_string()
    {
        // Bug: same SerializeDateConstraint fallthrough for LessThanOrEqualTo
        var desc = _adapter.ExtractRules(typeof(DateOnlyLessThanOrEqualToValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var rule = desc!.Fields[0].Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("max"));
        Assert.That(rule.CoerceAs, Is.EqualTo("date"),
            "InferCoerceAs should map DateOnly to 'date'");

        // THIS ASSERTION FAILS TODAY — constraint is a raw DateOnly object, not a string
        Assert.That(rule.Constraint, Is.InstanceOf<string>(),
            "DateOnly constraint must be serialized as a string, not a raw DateOnly object");
        Assert.That(rule.Constraint, Is.EqualTo("2026-12-31"),
            "DateOnly(2026, 12, 31) must serialize to 'yyyy-MM-dd' format");
    }

    [Test]
    public void DateOnly_InclusiveBetween_from_and_to_are_both_serialized_as_yyyy_MM_dd_strings()
    {
        // Bug: MapRangeValidator calls SerializeDateConstraint for both bounds,
        // but SerializeDateConstraint falls through for DateOnly — returns raw objects.
        var desc = _adapter.ExtractRules(typeof(DateOnlyRangeValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var rule = desc!.Fields[0].Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("range"));
        Assert.That(rule.CoerceAs, Is.EqualTo("date"),
            "InferCoerceAs should map DateOnly to 'date'");

        var constraint = rule.Constraint as object[];
        Assert.That(constraint, Is.Not.Null, "range constraint must be an array");
        Assert.That(constraint!, Has.Length.EqualTo(2));

        // THESE ASSERTIONS FAIL TODAY — both bounds are raw DateOnly objects, not strings
        Assert.That(constraint[0], Is.InstanceOf<string>(),
            "Range 'from' bound must be serialized as a string, not a raw DateOnly object");
        Assert.That(constraint[0], Is.EqualTo("2026-01-01"),
            "DateOnly(2026, 1, 1) must serialize to 'yyyy-MM-dd' format");

        Assert.That(constraint[1], Is.InstanceOf<string>(),
            "Range 'to' bound must be serialized as a string, not a raw DateOnly object");
        Assert.That(constraint[1], Is.EqualTo("2026-12-31"),
            "DateOnly(2026, 12, 31) must serialize to 'yyyy-MM-dd' format");
    }
}
