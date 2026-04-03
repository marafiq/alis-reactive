using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionDateRangePicker : FusionTestBase
{
    [Test]
    public Task StartDate_returns_component_value_expression_with_startDate_value_member_path()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayPeriod).StartDate();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<DateTime>>());
            Assert.That(source.CoercionType, Is.EqualTo("date"));
            Assert.That(source.ElementCoercionType, Is.Null);
            p.Element("start-echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__StayPeriod"));
        Assert.That(json, Does.Contain("startDate"));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task EndDate_returns_component_value_expression_with_endDate_value_member_path()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayPeriod).EndDate();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<DateTime>>());
            Assert.That(source.CoercionType, Is.EqualTo("date"));
            Assert.That(source.ElementCoercionType, Is.Null);
            p.Element("end-echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__StayPeriod"));
        Assert.That(json, Does.Contain("endDate"));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Value_returns_component_value_expression_with_value_member_path_and_DateTime_array()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayPeriod).Value();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<DateTime[]>>());
            Assert.That(source.CoercionType, Is.EqualTo("array"));
            Assert.That(source.ElementCoercionType, Is.EqualTo("date"));
            p.Element("range-echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__StayPeriod"));
        Assert.That(json, Does.Contain("value"));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task StartDate_source_in_SetText_produces_correct_plan()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var comp = p.Component<FusionDateRangePicker>(m => m.StayPeriod);
            p.Element("start-echo").SetText(comp.StartDate());
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task EndDate_source_in_SetText_produces_correct_plan()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var comp = p.Component<FusionDateRangePicker>(m => m.StayPeriod);
            p.Element("end-echo").SetText(comp.EndDate());
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
