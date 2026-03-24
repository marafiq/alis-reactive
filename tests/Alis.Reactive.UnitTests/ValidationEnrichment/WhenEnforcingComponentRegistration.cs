using System.IO;
using Alis.Reactive.InputField;

namespace Alis.Reactive.UnitTests.ValidationEnrichment;

/// <summary>
/// Overview #3 — Component registration enforcement.
/// InputFieldSetup.Render() must throw if the component was not registered
/// via AddToComponentsMap. Silent skips in validation and gather are forbidden.
/// </summary>
[TestFixture]
public class WhenEnforcingComponentRegistration
{
    [Test]
    public void Render_throws_when_component_not_registered()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        var writer = new StringWriter();

        var setup = new InputFieldSetup<object, EnrichmentTestModel, string>(
            new object(),
            plan,
            x => x.Name!,
            new InputFieldOptions(),
            "Name",
            "Name",
            writer);

        Assert.Throws<InvalidOperationException>(() =>
            setup.Render(() => writer.Write("<input />")));
    }

    [Test]
    public void Render_succeeds_when_component_registered()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        var writer = new StringWriter();

        plan.AddToComponentsMap("Name", new ComponentRegistration(
            "Name", "native", "Name", "value", "textbox"));

        var setup = new InputFieldSetup<object, EnrichmentTestModel, string>(
            new object(),
            plan,
            x => x.Name!,
            new InputFieldOptions(),
            "Name",
            "Name",
            writer);

        Assert.DoesNotThrow(() =>
            setup.Render(() => writer.Write("<input />")));
    }

    [Test]
    public void Render_error_message_includes_binding_path()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        var writer = new StringWriter();

        var setup = new InputFieldSetup<object, EnrichmentTestModel, string>(
            new object(),
            plan,
            x => x.Email!,
            new InputFieldOptions(),
            "Email",
            "Email",
            writer);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            setup.Render(() => writer.Write("<input />")));
        Assert.That(ex!.Message, Does.Contain("Email"));
        Assert.That(ex.Message, Does.Contain("AddToComponentsMap"));
    }
}
