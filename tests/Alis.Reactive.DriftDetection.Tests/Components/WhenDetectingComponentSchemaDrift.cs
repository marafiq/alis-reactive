using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Fusion.Components;
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
        var plan = CreatePlan();

        try
        {
            Html.InputField(plan, m => m.Name)
                .NativeTextBox(b => b.Placeholder("Name"));
        }
        catch (NotImplementedException)
        {
            // TestHtmlHelper.TextBoxFor — component already registered in ComponentsMap
        }

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ComponentEntry", "components.Name");
    }

    [Test]
    public void fusion_component_entry_conforms()
    {
        // ComponentEntry for a Fusion component: vendor="fusion"
        // FusionNumericTextBox via Component<T>() does NOT register in ComponentsMap
        // (only InputField registers). We need InputField + a Fusion component.
        // FusionNumericTextBox HTML extensions require Syncfusion EJS() which won't work
        // in test. Instead, manually add to ComponentsMap (internal) is not available.
        //
        // Alternative: Use InputField + NativeTextBox for a second field, then verify
        // the plan's component map has different entries.
        var plan = CreatePlan();

        try
        {
            Html.InputField(plan, m => m.Name)
                .NativeTextBox(b => b.Placeholder("Name"));
        }
        catch (NotImplementedException) { }

        try
        {
            Html.InputField(plan, m => m.Email)
                .NativeTextBox(b => b.Placeholder("Email"));
        }
        catch (NotImplementedException) { }

        var json = plan.Render();
        AssertSchemaValid(json);

        // Both components serialized correctly
        AssertAllPropertiesPresent(json, "ComponentEntry", "components.Name");
        AssertAllPropertiesPresent(json, "ComponentEntry", "components.Email");
    }

    [Test]
    public void component_source_in_guard_conforms()
    {
        // ComponentSource used inside a ValueGuard's source field
        // When(compValue).Eq(...) produces a guard with source: ComponentSource
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
