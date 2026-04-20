using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenReadingFusionInPlaceEditorValue : FusionTestBase
{
    [Test]
    public void Value_returns_typed_component_source_of_string()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<string>>());
        });
    }

    [Test]
    public void Value_read_is_reflected_in_rendered_plan()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var comp = p.Component<FusionInPlaceEditor>(m => m.PhoneNumber);
            p.Element("echo").SetText(comp.Value());
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"value\""), "Must reference the 'value' member");
    }

}
