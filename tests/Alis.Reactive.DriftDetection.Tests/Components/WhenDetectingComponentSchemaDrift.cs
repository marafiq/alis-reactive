using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.DriftDetection.Tests.Components;

[TestFixture]
public class WhenDetectingComponentSchemaDrift : DriftTestBase
{
    [Test]
    public void native_component_entry_conforms()
    {
        // ComponentEntry: id, vendor, readExpr, componentType, coerceAs
        // NativeTextBox registered via InputField adds to plan.components
        AssertDefinitionPropertiesExactly("ComponentEntry",
            "id", "vendor", "readExpr", "componentType", "coerceAs");

        var plan = CreatePlan();

        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b.Placeholder("Name"));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ComponentEntry", "components.Name");
    }

    [Test]
    public void fusion_component_entry_conforms()
    {
        // Fusion component registration requires Syncfusion's EJS() helper, which needs
        // a real MVC service context. The minimal TestHtmlHelper intentionally does not
        // provide that infrastructure, so this public path cannot be exercised here.
        Assert.Inconclusive(
            "Fusion component registration requires Syncfusion EJS infrastructure " +
            "not available in drift detection tests.");
    }

    [Test]
    public void component_source_in_guard_conforms()
    {
        // ComponentSource used inside a ValueGuard's source field
        // When(compValue).Eq(...) produces a guard with source: ComponentSource
        AssertDefinitionPropertiesExactly("ComponentSource",
            "kind", "componentId", "vendor", "readExpr");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            var nameComp = p.Component<NativeTextBox>(m => m.Name);
            p.When(nameComp.Value()).NotEmpty()
             .Then(tp => tp.Element("name-filled").Show())
             .Else(ep => ep.Element("name-filled").Hide());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        // The guard's source is a ComponentSource
        AssertAllPropertiesPresent(json, "ComponentSource",
            "entries[0].reaction.branches[0].guard.source");
    }
}
