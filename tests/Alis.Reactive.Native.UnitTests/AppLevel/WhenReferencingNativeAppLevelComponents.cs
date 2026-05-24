using System.Text.Json;
using Alis.Reactive.Native.AppLevel;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenReferencingNativeAppLevelComponents : NativeTestBase
{
    [Test]
    public void Default_app_component_reference_emits_layout_object_contribution()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Component<NativeLoader>().Show());

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var component = doc.RootElement
            .GetProperty("components")
            .GetProperty(NativeLoader.ElementId);

        Assert.Multiple(() =>
        {
            Assert.That(component.GetProperty("contribution").GetProperty("kind").GetString(), Is.EqualTo("layout-object"));
            Assert.That(component.GetProperty("binding").GetProperty("kind").GetString(), Is.EqualTo("none"));
            Assert.That(component.GetProperty("container").GetProperty("kind").GetString(), Is.EqualTo("none"));
        });
    }

    [Test]
    public void Explicit_id_app_component_reference_stays_object_target()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Component<NativeLoader>(NativeLoader.ElementId).Show());

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var component = doc.RootElement
            .GetProperty("components")
            .GetProperty(NativeLoader.ElementId);

        Assert.That(component.GetProperty("contribution").GetProperty("kind").GetString(), Is.EqualTo("object-target"));
    }
}
