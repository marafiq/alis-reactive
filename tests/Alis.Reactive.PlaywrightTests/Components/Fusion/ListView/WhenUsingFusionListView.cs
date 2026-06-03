using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ListView;

[TestFixture]
public class WhenUsingFusionListView : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionListView";

    private FusionListViewLocator Residents => new(Page, "resident-list");
    private FusionListViewLocator Tasks => new(Page, "task-list");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#command-state", "pending");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionListView — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_text_and_unselect_text_methods_update_selection()
    {
        await NavigateAndBoot();

        await Page.Locator("#select-bennett-btn").ClickAsync();
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("selected Bennett", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Bennett"), Is.True);

        await Page.Locator("#unselect-bennett-btn").ClickAsync();
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("unselected Bennett", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Bennett"), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task check_all_and_uncheck_all_methods_update_checkbox_items()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-all-btn").ClickAsync();
        await Expect(Page.Locator("#task-command-state")).ToHaveTextAsync("checked all", new() { Timeout = 5000 });
        await Expect(Tasks.CheckedIcon("Hydration")).ToHaveCountAsync(1);
        await Expect(Tasks.CheckedIcon("Mobility")).ToHaveCountAsync(1);
        await Expect(Tasks.CheckedIcon("Dining")).ToHaveCountAsync(1);

        await Page.Locator("#uncheck-all-btn").ClickAsync();
        await Expect(Page.Locator("#task-command-state")).ToHaveTextAsync("unchecked all", new() { Timeout = 5000 });
        await Expect(Tasks.CheckedIcon("Hydration")).ToHaveCountAsync(0);
        await Expect(Tasks.CheckedIcon("Mobility")).ToHaveCountAsync(0);
        await Expect(Tasks.CheckedIcon("Dining")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_event_reads_scalar_payload_and_conditions()
    {
        await NavigateAndBoot();

        await Residents.ClickItem("Alice");

        await Expect(Page.Locator("#selected-text")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-index")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-kind")).ToHaveTextAsync("primary", new() { Timeout = 5000 });
        await Expect(Page.Locator("#cancel-state")).ToHaveTextAsync("allowed", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Alice"), Is.True);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_event_cancel_mutation_prevents_selection()
    {
        await NavigateAndBoot();

        await Residents.ClickItem("Carey");

        await Expect(Page.Locator("#selected-text")).ToHaveTextAsync("Carey", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-index")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#cancel-state")).ToHaveTextAsync("cancelled", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Carey"), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checkbox_selected_event_reads_checked_state()
    {
        await NavigateAndBoot();

        await Tasks.ClickItem("Hydration");

        await Expect(Page.Locator("#task-selected-text")).ToHaveTextAsync("Hydration", new() { Timeout = 5000 });
        await Expect(Page.Locator("#task-checked")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#task-check-state")).ToHaveTextAsync("checked", new() { Timeout = 5000 });
        await Expect(Tasks.CheckedIcon("Hydration")).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }
}
