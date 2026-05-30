namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// The array DSL over NATIVE DOM, end-to-end in the browser. A DOM element resolved by
/// getElementById is a JS object; its classList (DOMTokenList) and children (HTMLCollection)
/// are array-likes the runtime normalizes, so the closed array ops apply directly:
///   p.FromDom("dom-card", "classList").Count()
///   p.FromDom("dom-card", "classList").Where(x => x.StartsWith("risk-")).Count()
///   p.FromDom("dom-list", "children").Count()
///   p.FromDom("dom-card", "classList").Any(x => x == "care-memory")  -> guard
/// and DOM mutation + recompute in one tick (AddClass then re-count).
/// No plugin, no hand-written JS.
///
/// Page under test: /Sandbox/Components/DomOps. Isolated slice.
/// </summary>
[TestFixture]
public class WhenDomOpsTransformElements : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DomOps";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task counts_the_css_classes_of_a_dom_element()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-total-classes")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task counts_risk_classes_by_prefix_filter_over_classList()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-risk-count")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task counts_child_elements_of_a_dom_collection()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-child-count")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flags_a_memory_care_class_via_any_predicate_over_classList()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-memory-yes")).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recounts_risk_classes_after_mutating_the_dom_element()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-risk-count")).ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Page.Locator("#dom-add-risk-btn").ClickAsync();

        await Expect(Page.Locator("#dom-risk-count")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
