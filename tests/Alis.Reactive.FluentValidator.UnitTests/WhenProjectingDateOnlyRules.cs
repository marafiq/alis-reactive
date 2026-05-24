using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

/// <summary>
/// DateOnly validation constraints serialize as yyyy-MM-dd plan literals.
/// </summary>
[TestFixture]
public class WhenProjectingDateOnlyRules
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
        var desc = _adapter.ProjectRules(typeof(DateOnlyGreaterThanOrEqualToValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var rule = desc[0].Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("min"));
        Assert.That(rule.Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.Date),
            "Shape.FromClrType should map DateOnly to 'date'");

        Assert.That(rule.ConstraintValue(), Is.InstanceOf<string>(),
            "DateOnly constraint must be serialized as a string, not a raw DateOnly object");
        Assert.That(rule.ConstraintValue(), Is.EqualTo("2026-01-01"),
            "DateOnly(2026, 1, 1) must serialize to 'yyyy-MM-dd' format");
    }

    [Test]
    public void DateOnly_LessThanOrEqualTo_constraint_is_serialized_as_yyyy_MM_dd_string()
    {
        var desc = _adapter.ProjectRules(typeof(DateOnlyLessThanOrEqualToValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var rule = desc[0].Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("max"));
        Assert.That(rule.Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.Date),
            "Shape.FromClrType should map DateOnly to 'date'");

        Assert.That(rule.ConstraintValue(), Is.InstanceOf<string>(),
            "DateOnly constraint must be serialized as a string, not a raw DateOnly object");
        Assert.That(rule.ConstraintValue(), Is.EqualTo("2026-12-31"),
            "DateOnly(2026, 12, 31) must serialize to 'yyyy-MM-dd' format");
    }

    [Test]
    public void DateOnly_InclusiveBetween_from_and_to_are_both_serialized_as_yyyy_MM_dd_strings()
    {
        var desc = _adapter.ProjectRules(typeof(DateOnlyRangeValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var rule = desc[0].Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("range"));
        Assert.That(rule.Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.Date),
            "Shape.FromClrType should map DateOnly to 'date'");

        var constraint = rule.ConstraintValue() as object[];
        Assert.That(constraint, Is.Not.Null, "range constraint must be an array");
        Assert.That(constraint!, Has.Length.EqualTo(2));

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
