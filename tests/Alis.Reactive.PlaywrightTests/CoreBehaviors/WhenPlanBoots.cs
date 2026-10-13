namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

[TestFixture]
public class WhenPlanBoots : PlaywrightTestBase
{
    [Test]
    public async Task home_page_links_to_sandbox_events()
    {
        // The site root redirects to the hierarchical Sandbox hub, the single top of the tree.
        await NavigateTo("/");
        await Expect(Page).ToHaveTitleAsync("Sandbox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();

        // Drill down the hierarchy by cards: hub -> Core Behaviors section -> Events demo.
        await Page.GetByRole(AriaRole.Link, new() { Name = "Core Behaviors" }).ClickAsync();
        await Page.WaitForURLAsync("**/Sandbox/CoreBehaviors");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Events" }).ClickAsync();
        await Page.WaitForURLAsync("**/Sandbox/CoreBehaviors/Events");
        await Expect(Page).ToHaveTitleAsync("Events & Dispatch — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task events_page_renders_event_chain_set_reaction_targets()
    {
        await NavigateTo("/Sandbox/CoreBehaviors/Events");

        await Expect(Page.Locator("#step-1")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync();
        await Expect(Page.Locator("#chain-status")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task events_page_renders_with_correct_title()
    {
        await NavigateTo("/Sandbox/CoreBehaviors/Events");
        await Expect(Page).ToHaveTitleAsync("Events & Dispatch — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }
}
