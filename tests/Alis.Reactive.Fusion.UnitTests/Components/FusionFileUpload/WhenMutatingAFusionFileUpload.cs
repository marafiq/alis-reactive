using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionFileUpload : FusionTestBase
{
    [Test]
    public Task Value_returns_component_value_expression()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionFileUpload>(m => m.Documents).Value();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<string>>());
            Assert.That(source.CoercionType, Is.EqualTo("string"));
            p.Element("echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__Documents"));
        Assert.That(json, Does.Contain("filesData"));
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
