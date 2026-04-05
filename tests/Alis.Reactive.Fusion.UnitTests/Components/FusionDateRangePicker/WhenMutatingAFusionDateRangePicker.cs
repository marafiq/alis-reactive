using Alis.Reactive.PlanModel;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionDateRangePicker : FusionTestBase
{
    [Test]
    public void StartDate_returns_typed_component_source_with_startDate_valueMember()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayPeriod).StartDate();
            Assert.That(source, Is.TypeOf<TypedComponentSource<DateTime>>());
        });
    }

    [Test]
    public void EndDate_returns_typed_component_source_with_endDate_valueMember()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayPeriod).EndDate();
            Assert.That(source, Is.TypeOf<TypedComponentSource<DateTime>>());
        });
    }

    [Test]
    public void Value_returns_typed_component_source_with_value_valueMember_and_DateTime_array()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayPeriod).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<DateTime[]>>());
        });
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
