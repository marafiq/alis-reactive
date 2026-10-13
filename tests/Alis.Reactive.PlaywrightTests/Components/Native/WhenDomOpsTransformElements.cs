namespace Alis.Reactive.PlaywrightTests.Components.Native;

// DOM classList and children are normalized so Count and Where work without a
// plugin or hand-written JavaScript.
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
    public async Task the_care_card_carries_four_care_flags()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-total-classes")).ToHaveTextAsync("4", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The card also carries "fall-risk-reviewed", which holds "risk-" after its start: StartsWith counts
    // risk-fall and risk-oxygen; a Contains in its place would count 3, an EndsWith 0.
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

    // "low-sodium", "sodium-free", "reduced-sodium": Contains counts 3, where StartsWith would count 1 and
    // EndsWith 2.
    [Test]
    public async Task counts_every_diet_that_mentions_sodium_anywhere_in_its_name()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-sodium-diets")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // EndsWith("sodium") counts the low and reduced sodium diets, 2, where Contains would count 3 and
    // StartsWith 1 (sodium-free).
    [Test]
    public async Task counts_low_and_reduced_sodium_diets_by_how_their_names_end()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#dom-limited-sodium")).ToHaveTextAsync("2", new() { Timeout = 5000 });
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
