using Alis.Reactive.PlanModel;
using Alis.Reactive.Builders.Conditions;
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
        });
    }
}
