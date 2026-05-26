using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingNewOperatorConditions
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    // ── Ordering operators ─────────────────────────────────────────────────

    [Test]
    public void WhenFieldGt_projects_gt_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldGtValidator), "form");
        var jobTitle = fields.First(f => f.FieldName == "JobTitle");
        var when = (FieldCompare)jobTitle.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Age"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.GreaterThan));
        Assert.That(when.OperandValue(), Is.EqualTo(18));
    }

    [Test]
    public void WhenFieldGte_projects_gte_condition_with_decimal()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldGteValidator), "form");
        var email = fields.First(f => f.FieldName == "Email");
        var when = (FieldCompare)email.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Salary"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.GreaterThanOrEqual));
        Assert.That(when.OperandValue(), Is.EqualTo(50000m));
    }

    [Test]
    public void WhenFieldLt_projects_lt_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldLtValidator), "form");
        var name = fields.First(f => f.FieldName == "Name");
        var when = (FieldCompare)name.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Age"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.LessThan));
        Assert.That(when.OperandValue(), Is.EqualTo(18));
    }

    [Test]
    public void WhenFieldLte_projects_lte_condition_with_decimal()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldLteValidator), "form");
        var notes = fields.First(f => f.FieldName == "Notes");
        var when = (FieldCompare)notes.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Salary"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.LessThanOrEqual));
        Assert.That(when.OperandValue(), Is.EqualTo(0m));
    }

    // ── Presence operators ─────────────────────────────────────────────────

    [Test]
    public void WhenFieldNull_projects_is_null_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldNullValidator), "form");
        var notes = fields.First(f => f.FieldName == "Notes");
        var when = (FieldCompare)notes.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("MiddleName"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.IsNull));
        Assert.That(when.Operand, Is.InstanceOf<NoFieldComparisonOperand>());
    }

    [Test]
    public void WhenFieldNotNull_projects_not_null_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldNotNullValidator), "form");
        var name = fields.First(f => f.FieldName == "Name");
        var when = (FieldCompare)name.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("MiddleName"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.NotNull));
        Assert.That(when.Operand, Is.InstanceOf<NoFieldComparisonOperand>());
    }

    [Test]
    public void WhenFieldEmpty_projects_is_empty_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldEmptyValidator), "form");
        var phone = fields.First(f => f.FieldName == "Phone");
        var when = (FieldCompare)phone.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Email"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.IsEmpty));
        Assert.That(when.Operand, Is.InstanceOf<NoFieldComparisonOperand>());
    }

    [Test]
    public void WhenFieldNotEmpty_projects_not_empty_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldNotEmptyValidator), "form");
        var name = fields.First(f => f.FieldName == "Name");
        var when = (FieldCompare)name.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Notes"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.NotEmpty));
        Assert.That(when.Operand, Is.InstanceOf<NoFieldComparisonOperand>());
    }

    // ── Membership operators ───────────────────────────────────────────────

    [Test]
    public void WhenFieldIn_projects_in_condition_with_array_value()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldInValidator), "form");
        var notes = fields.First(f => f.FieldName == "Notes");
        var when = (FieldCompare)notes.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("CareLevel"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.In));
        Assert.That(when.OperandValue(), Is.InstanceOf<object[]>());
        var values = (object[])when.OperandValue()!;
        Assert.That(values, Is.EqualTo(new object[] { "memory-care", "skilled-nursing" }));
    }

    [Test]
    public void WhenFieldNotIn_projects_not_in_condition_with_array_value()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldNotInValidator), "form");
        var phone = fields.First(f => f.FieldName == "Phone");
        var when = (FieldCompare)phone.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("CareLevel"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.NotIn));
        Assert.That(when.OperandValue(), Is.InstanceOf<object[]>());
        var values = (object[])when.OperandValue()!;
        Assert.That(values, Is.EqualTo(new object[] { "independent", "assisted" }));
    }

    [Test]
    public void WhenFieldBetween_projects_between_condition_with_range()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldBetweenValidator), "form");
        var jobTitle = fields.First(f => f.FieldName == "JobTitle");
        var when = (FieldCompare)jobTitle.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Age"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.Between));
        Assert.That(when.OperandValue(), Is.InstanceOf<object[]>());
        var range = (object[])when.OperandValue()!;
        Assert.That(range[0], Is.EqualTo(18));
        Assert.That(range[1], Is.EqualTo(65));

        var planCondition = ResolveWithNumberShape(when);
        AssertRightArrayOperandShape(planCondition, Shape.ArrayOf(Shape.Number));
    }

    [Test]
    public void WhenFieldBetween_keeps_array_operand_shape_when_field_shape_is_unspecified()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldBetweenValidator), "form");
        var jobTitle = fields.First(f => f.FieldName == "JobTitle");
        var when = (FieldCompare)jobTitle.Rules[0].WhenCondition();

        var planCondition = ResolveWithShape(when, Shape.None);

        Assert.That(planCondition.Shape, Is.EqualTo(Shape.None));
        AssertRightArrayOperandShape(planCondition, Shape.ArrayOf(Shape.Any));
    }

    // ── Text operators ─────────────────────────────────────────────────────

    [Test]
    public void WhenFieldContains_projects_contains_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldContainsValidator), "form");
        var phone = fields.First(f => f.FieldName == "Phone");
        var when = (FieldCompare)phone.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Notes"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.Contains));
        Assert.That(when.OperandValue(), Is.EqualTo("urgent"));
    }

    [Test]
    public void WhenFieldStartsWith_projects_starts_with_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldStartsWithValidator), "form");
        var email = fields.First(f => f.FieldName == "Email");
        var when = (FieldCompare)email.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Name"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.StartsWith));
        Assert.That(when.OperandValue(), Is.EqualTo("Dr."));
    }

    [Test]
    public void WhenFieldEndsWith_projects_ends_with_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldEndsWithValidator), "form");
        var jobTitle = fields.First(f => f.FieldName == "JobTitle");
        var when = (FieldCompare)jobTitle.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Email"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.EndsWith));
        Assert.That(when.OperandValue(), Is.EqualTo("@hospital.org"));
    }

    // ── Condition source fields included ───────────────────────────────────

    [Test]
    public void Condition_source_fields_are_included_for_all_operators()
    {
        // Every WhenField* condition field must appear in the projected fields
        // so the runtime can read its value.
        void AssertSourceField(Type validatorType, string expectedSourceField)
        {
            var fields = _adapter.ProjectRules(validatorType, "form");
            var fieldNames = fields.Select(f => f.FieldName).ToList();
            Assert.That(fieldNames, Does.Contain(expectedSourceField),
                $"{validatorType.Name}: condition source field '{expectedSourceField}' missing");
        }

        AssertSourceField(typeof(WhenFieldGtValidator), "Age");
        AssertSourceField(typeof(WhenFieldGteValidator), "Salary");
        AssertSourceField(typeof(WhenFieldLtValidator), "Age");
        AssertSourceField(typeof(WhenFieldLteValidator), "Salary");
        AssertSourceField(typeof(WhenFieldNullValidator), "MiddleName");
        AssertSourceField(typeof(WhenFieldNotNullValidator), "MiddleName");
        AssertSourceField(typeof(WhenFieldEmptyValidator), "Email");
        AssertSourceField(typeof(WhenFieldNotEmptyValidator), "Notes");
        AssertSourceField(typeof(WhenFieldInValidator), "CareLevel");
        AssertSourceField(typeof(WhenFieldNotInValidator), "CareLevel");
        AssertSourceField(typeof(WhenFieldBetweenValidator), "Age");
        AssertSourceField(typeof(WhenFieldContainsValidator), "Notes");
        AssertSourceField(typeof(WhenFieldStartsWithValidator), "Name");
        AssertSourceField(typeof(WhenFieldEndsWithValidator), "Email");
        AssertSourceField(typeof(WhenFieldMatchesValidator), "Phone");
        AssertSourceField(typeof(WhenFieldMinLengthValidator), "Notes");
        AssertSourceField(typeof(WhenFieldArrayContainsValidator), "Tags");
    }

    // ── Matches / MinLength / ArrayContains operators ──────────────────

    [Test]
    public void WhenFieldMatches_projects_matches_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldMatchesValidator), "form");
        var name = fields.First(f => f.FieldName == "Name");
        var when = (FieldCompare)name.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Phone"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.Matches));
        Assert.That(when.OperandValue(), Is.EqualTo(@"^\d{3}-"));
    }

    [Test]
    public void WhenFieldMinLength_projects_min_length_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldMinLengthValidator), "form");
        var email = fields.First(f => f.FieldName == "Email");
        var when = (FieldCompare)email.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Notes"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.MinLength));
        Assert.That(when.OperandValue(), Is.EqualTo(10));
    }

    [Test]
    public void WhenFieldMinLength_rejects_negative_lengths_before_projection()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NegativeWhenFieldMinLengthValidator());

        Assert.That(exception!.ParamName, Is.EqualTo("minLength"));
        Assert.That(exception.Message, Does.Contain("zero or greater"));
    }

    [Test]
    public void WhenFieldArrayContains_projects_array_contains_condition()
    {
        var fields = _adapter.ProjectRules(typeof(WhenFieldArrayContainsValidator), "form");
        var phone = fields.First(f => f.FieldName == "Phone");
        var when = (FieldCompare)phone.Rules[0].WhenCondition();

        Assert.That(when.Field, Is.EqualTo("Tags"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.ArrayContains));
        Assert.That(when.OperandValue(), Is.EqualTo("urgent"));

        var planCondition = ResolveWithArrayShape(when);
        Assert.That(planCondition.Shape, Is.EqualTo(Shape.ArrayOf(Shape.String)));
        Assert.That(planCondition.ItemShape, Is.EqualTo(Shape.String));
    }

    private static CompareCondition ResolveWithArrayShape(FieldCompare when)
    {
        var arrayShape = Shape.ArrayOf(Shape.String);
        var binding = new FieldConditionPlanBinding(_ =>
            FieldComparisonTarget.ForComponentValue(
                ValueProducer.LiteralRaw(new[] { "routine", "urgent" }, arrayShape),
                arrayShape));

        return (CompareCondition)when.ToPlanCondition(binding);
    }

    private static CompareCondition ResolveWithNumberShape(FieldCompare when)
    {
        return ResolveWithShape(when, Shape.Number);
    }

    private static CompareCondition ResolveWithShape(FieldCompare when, Shape shape)
    {
        var binding = new FieldConditionPlanBinding(_ =>
            FieldComparisonTarget.ForComponentValue(
                ValueProducer.LiteralRaw(42, shape),
                shape));

        return (CompareCondition)when.ToPlanCondition(binding);
    }

    private static void AssertRightArrayOperandShape(CompareCondition condition, Shape expected)
    {
        Assert.That(condition.RightOperand, Is.InstanceOf<PresentComparisonRightOperand>());

        var right = (PresentComparisonRightOperand)condition.RightOperand;
        Assert.That(right.Value, Is.InstanceOf<ArrayProducer>());

        var array = (ArrayProducer)right.Value;
        Assert.That(array.Shape, Is.EqualTo(expected));
    }
}
