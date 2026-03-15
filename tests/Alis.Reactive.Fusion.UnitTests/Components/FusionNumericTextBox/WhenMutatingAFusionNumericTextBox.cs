using Alis.Reactive;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Fusion.Components;
using static VerifyNUnit.Verifier;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionNumericTextBox : FusionTestBase
{
    [Test]
    public Task SetValue_produces_ej2_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(42));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_ej2_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).FocusIn());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task SetValue_followed_by_element_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(100);
            p.Element("echo").SetText("Amount updated");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Value_returns_typed_component_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionNumericTextBox>(m => m.Amount).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<decimal>>());

            var bindSource = source.ToBindSource();
            Assert.That(bindSource, Is.TypeOf<ComponentSource>());

            var cs = (ComponentSource)bindSource;
            Assert.That(cs.ComponentId, Is.EqualTo("Alis_Reactive_Fusion_UnitTests_FusionTestModel__Amount"));
            Assert.That(cs.Vendor, Is.EqualTo("fusion"));
            Assert.That(cs.ReadExpr, Is.EqualTo("value"));
        });
    }
}
