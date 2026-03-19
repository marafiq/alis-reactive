using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.UnitTests;

public class DescriptorPayload
{
    public string? Value { get; set; }
    public string[]? Tags { get; set; }
    public decimal Rate { get; set; }
}

/// <summary>
/// BDD tests covering descriptor paths not hit by higher-level tests:
/// TypedSource.ElementCoercionType, ValueGuard constructors,
/// ElementBuilder source overloads, per-action When guards, SourceArg.
/// </summary>
[TestFixture]
public class WhenBuildingDescriptors : PlanTestBase
{
    // ═══════════════════════════════════════════════════════════
    // TypedSource — ElementCoercionType for arrays
    // ═══════════════════════════════════════════════════════════

    [Test]
    public void String_array_source_has_string_element_coercion()
    {
        var source = new EventArgSource<DescriptorPayload, string[]>(x => x.Tags);
        Assert.That(source.CoercionType, Is.EqualTo("array"));
        Assert.That(source.ElementCoercionType, Is.EqualTo("string"));
    }

    [Test]
    public void Scalar_source_has_null_element_coercion()
    {
        var source = new EventArgSource<DescriptorPayload, string>(x => x.Value);
        Assert.That(source.CoercionType, Is.EqualTo("string"));
        Assert.That(source.ElementCoercionType, Is.Null);
    }

    [Test]
    public void Decimal_source_has_number_coercion()
    {
        var source = new EventArgSource<DescriptorPayload, decimal>(x => x.Rate);
        Assert.That(source.CoercionType, Is.EqualTo("number"));
        Assert.That(source.ElementCoercionType, Is.Null);
    }

    // ═══════════════════════════════════════════════════════════
    // ElementBuilder — SetText/SetHtml with BindSource and TypedSource
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task SetText_with_event_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DescriptorPayload>("test", (args, p) =>
            p.Element("echo").SetText(args, x => x.Value));
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task SetHtml_with_event_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DescriptorPayload>("test", (args, p) =>
            p.Element("container").SetHtml(args, x => x.Value));
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task SetText_with_bind_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DescriptorPayload>("test", (args, p) =>
        {
            var source = new EventSource("evt.value");
            p.Element("echo").SetText(source);
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task SetHtml_with_bind_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DescriptorPayload>("test", (args, p) =>
        {
            var source = new EventSource("evt.value");
            p.Element("container").SetHtml(source);
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task SetText_with_typed_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DescriptorPayload>("test", (args, p) =>
        {
            var typedSource = new EventArgSource<DescriptorPayload, string>(x => x.Value);
            p.Element("echo").SetText(typedSource);
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task SetHtml_with_typed_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DescriptorPayload>("test", (args, p) =>
        {
            var typedSource = new EventArgSource<DescriptorPayload, string>(x => x.Value);
            p.Element("container").SetHtml(typedSource);
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ═══════════════════════════════════════════════════════════
    // ElementBuilder — Show, Hide (return type regression)
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Show_and_hide_in_sequence()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("panel").Show();
            p.Element("panel").Hide();
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task AddClass_RemoveClass_ToggleClass()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("box").AddClass("active");
            p.Element("box").RemoveClass("hidden");
            p.Element("box").ToggleClass("highlight");
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ═══════════════════════════════════════════════════════════
    // Per-action When guard on ElementBuilder
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Dispatch_without_payload()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Dispatch("page-loaded"));
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ═══════════════════════════════════════════════════════════
    // Source-vs-source comparison
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Source_vs_source_eq_comparison()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DescriptorPayload>("test", (args, p) =>
        {
            var left = new EventArgSource<DescriptorPayload, string>(x => x.Value);
            var right = new EventArgSource<DescriptorPayload, string>(x => x.Value);
            p.When(left).Eq(right)
                .Then(t => t.Element("r").SetText("same"));
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ═══════════════════════════════════════════════════════════
    // Dispatch with typed payload
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Dispatch_with_payload()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Dispatch("data-ready", new { count = 5, message = "loaded" }));
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ═══════════════════════════════════════════════════════════
    // ArrayContains guard
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task ArrayContains_on_string_array()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DescriptorPayload>("test", (args, p) =>
        {
            var source = new EventArgSource<DescriptorPayload, string[]>(x => x.Tags);
            p.When(source).ArrayContains("urgent")
                .Then(t => t.Element("alert").Show());
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }
}
