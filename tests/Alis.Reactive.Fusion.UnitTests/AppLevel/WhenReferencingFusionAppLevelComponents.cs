using System.Text.Json;
using Alis.Reactive.Fusion.AppLevel;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenReferencingFusionAppLevelComponents : FusionTestBase
{
    [Test]
    public void Default_app_component_reference_emits_layout_object_contribution()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Component<FusionToast>().SetTitle("Saved").Show());

        using var doc = JsonDocument.Parse(plan.RenderFormatted());
        var component = doc.RootElement
            .GetProperty("components")
            .GetProperty(FusionToast.ElementId);

        Assert.Multiple(() =>
        {
            Assert.That(component.GetProperty("vendor").GetString(), Is.EqualTo("fusion"));
            Assert.That(component.GetProperty("contribution").GetProperty("kind").GetString(), Is.EqualTo("layout-object"));
            Assert.That(component.GetProperty("binding").GetProperty("kind").GetString(), Is.EqualTo("none"));
            Assert.That(component.GetProperty("container").GetProperty("kind").GetString(), Is.EqualTo("none"));
        });
    }
}
