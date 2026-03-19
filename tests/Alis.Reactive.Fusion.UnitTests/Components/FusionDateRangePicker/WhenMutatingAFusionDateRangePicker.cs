using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionDateRangePicker : FusionTestBase
{
    [Test]
    public void StartDate_returns_typed_component_source_with_startDate_readExpr()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayStart).StartDate();
            Assert.That(source, Is.TypeOf<TypedComponentSource<DateTime>>());

            var bindSource = source.ToBindSource();
            Assert.That(bindSource, Is.TypeOf<ComponentSource>());

            var cs = (ComponentSource)bindSource;
            Assert.That(cs.ComponentId, Is.EqualTo("Alis_Reactive_Fusion_UnitTests_FusionTestModel__StayStart"));
            Assert.That(cs.Vendor, Is.EqualTo("fusion"));
            Assert.That(cs.ReadExpr, Is.EqualTo("startDate"));
        });
    }

    [Test]
    public void EndDate_returns_typed_component_source_with_endDate_readExpr()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayStart).EndDate();
            Assert.That(source, Is.TypeOf<TypedComponentSource<DateTime>>());

            var bindSource = source.ToBindSource();
            Assert.That(bindSource, Is.TypeOf<ComponentSource>());

            var cs = (ComponentSource)bindSource;
            Assert.That(cs.ComponentId, Is.EqualTo("Alis_Reactive_Fusion_UnitTests_FusionTestModel__StayStart"));
            Assert.That(cs.Vendor, Is.EqualTo("fusion"));
            Assert.That(cs.ReadExpr, Is.EqualTo("endDate"));
        });
    }

    [Test]
    public void Value_returns_same_as_StartDate()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDateRangePicker>(m => m.StayStart).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<DateTime>>());

            var bindSource = source.ToBindSource();
            Assert.That(bindSource, Is.TypeOf<ComponentSource>());

            var cs = (ComponentSource)bindSource;
            Assert.That(cs.ComponentId, Is.EqualTo("Alis_Reactive_Fusion_UnitTests_FusionTestModel__StayStart"));
            Assert.That(cs.Vendor, Is.EqualTo("fusion"));
            Assert.That(cs.ReadExpr, Is.EqualTo("startDate"));
        });
    }

    [Test]
    public Task StartDate_source_in_SetText_produces_correct_plan()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var comp = p.Component<FusionDateRangePicker>(m => m.StayStart);
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
            var comp = p.Component<FusionDateRangePicker>(m => m.StayStart);
            p.Element("end-echo").SetText(comp.EndDate());
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
