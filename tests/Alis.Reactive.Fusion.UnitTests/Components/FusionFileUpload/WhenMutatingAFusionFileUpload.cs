using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionFileUpload : FusionTestBase
{
    [Test]
    public void Value_returns_typed_component_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionFileUpload>(m => m.Documents).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<string>>());

            var bindSource = source.ToBindSource();
            Assert.That(bindSource, Is.TypeOf<ComponentSource>());

            var cs = (ComponentSource)bindSource;
            Assert.That(cs.ComponentId, Is.EqualTo("Alis_Reactive_Fusion_UnitTests_FusionTestModel__Documents"));
            Assert.That(cs.Vendor, Is.EqualTo("fusion"));
            Assert.That(cs.ReadExpr, Is.EqualTo("element.files"));
        });
    }
}
