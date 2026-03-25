using System.IO;
using Alis.Reactive.InputField;

namespace Alis.Reactive.UnitTests.ValidationEnrichment;

/// <summary>
/// Overview #3 — Component registration enforcement.
/// InputFieldSetup.Render() must throw if the component was not registered
/// via AddToComponentsMap. Silent skips in validation and gather are forbidden.
///
/// These tests use the internal InputFieldSetup constructor because the public
/// DSL (Html.InputField) requires ASP.NET IHtmlHelper, which lives in
/// Alis.Reactive.Native. The Native test project has integration tests that
/// exercise registration through the full public DSL.
/// </summary>
[TestFixture]
public class WhenEnforcingComponentRegistration
{
    private static InputFieldSetup<object, EnrichmentTestModel, string> CreateSetup(
        ReactivePlan<EnrichmentTestModel> plan,
        StringWriter writer,
        System.Linq.Expressions.Expression<System.Func<EnrichmentTestModel, string?>> expr,
        string bindingPath)
    {
        return new InputFieldSetup<object, EnrichmentTestModel, string>(
            new object(), plan, expr!, new InputFieldOptions(),
            bindingPath, bindingPath, writer);
    }

    [Test]
    public void Unregistered_component_throws_with_binding_path_and_fix_guidance()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        var writer = new StringWriter();
        var setup = CreateSetup(plan, writer, x => x.Name, "Name");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            setup.Render(() => writer.Write("<input />")));

        Assert.That(ex!.Message, Does.Contain("Name"),
            "Error must name the specific binding path that failed");
        Assert.That(ex.Message, Does.Contain("AddToComponentsMap"),
            "Error must tell the developer how to fix it");
    }

    [Test]
    public void Registered_component_renders_field_wrapper_with_label_and_validation_slot()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        var writer = new StringWriter();

        plan.AddToComponentsMap("Email", new ComponentRegistration(
            "Email", "native", "Email", "value", "textbox", "string"));

        var setup = CreateSetup(plan, writer, x => x.Email, "Email");
        setup.Options.Label("Email Address");
        setup.Render(() => writer.Write("<input type=\"email\" />"));

        var html = writer.ToString();
        Assert.That(html, Does.Contain("<label"),
            "Field wrapper must include a label element");
        Assert.That(html, Does.Contain("Email Address"),
            "Label must contain the configured text");
        Assert.That(html, Does.Contain("data-valmsg-for"),
            "Field wrapper must include a validation error slot");
        Assert.That(html, Does.Contain("<input type=\"email\" />"),
            "Child content must be rendered inside the wrapper");
    }

    [Test]
    public void Different_binding_paths_fail_independently()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        var writer = new StringWriter();

        // Register Name but not Email
        plan.AddToComponentsMap("Name", new ComponentRegistration(
            "Name", "native", "Name", "value", "textbox", "string"));

        var nameSetup = CreateSetup(plan, writer, x => x.Name, "Name");
        Assert.DoesNotThrow(() =>
            nameSetup.Render(() => writer.Write("<input />")),
            "Registered component must render successfully");

        var emailWriter = new StringWriter();
        var emailSetup = CreateSetup(plan, emailWriter, x => x.Email, "Email");
        var ex = Assert.Throws<InvalidOperationException>(() =>
            emailSetup.Render(() => emailWriter.Write("<input />")));
        Assert.That(ex!.Message, Does.Contain("Email"),
            "Error must name the unregistered binding path");
    }
}
