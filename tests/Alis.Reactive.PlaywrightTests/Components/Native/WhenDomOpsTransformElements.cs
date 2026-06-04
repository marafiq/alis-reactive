namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises array DSL operations over DOM array-like values.
/// </summary>
/// <remarks>
/// DOM <c>classList</c> and <c>children</c> are normalized so <c>Count</c>
/// and <c>Where</c> work without a plugin or hand-written JavaScript.
/// </remarks>
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

    [Test]
    public async Task counts_high_risk_children_by_calling_getAttribute_per_element()
    {
        // Per-element METHOD call: each child is a live DOM element; the plan calls
        // x.GetAttribute("data-risk") on each and counts those == "high" (2 of 3).
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-high-risk-count")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
