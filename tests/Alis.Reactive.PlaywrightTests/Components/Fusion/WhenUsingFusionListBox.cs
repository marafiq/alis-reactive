using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionListBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionListBox";

    private FusionListBoxLocator Residents => new(Page, "resident-list-box");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo", "pending");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionListBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_ready_set_value_and_value_source_condition_update_trace()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#value-state")).ToHaveTextAsync("has values", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Alice"), Is.True);
        Assert.That(await Residents.IsSelected("Bennett"), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_value_and_data_bind_update_selection()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-value-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("value set", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("bennett,dawson", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Alice"), Is.False);
        Assert.That(await Residents.IsSelected("Bennett"), Is.True);
        Assert.That(await Residents.IsSelected("Dawson"), Is.True);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_and_unselect_values_update_value_and_emit_change()
    {
        await NavigateAndBoot();

        await Page.Locator("#select-carey-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("carey selected", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("alice,carey", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-value")).ToHaveTextAsync("alice,carey", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-state")).ToHaveTextAsync("has values", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Carey"), Is.True);

        await Page.Locator("#unselect-carey-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("carey unselected", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-value")).ToHaveTextAsync("alice", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Carey"), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_all_and_unselect_all_update_values()
    {
        await NavigateAndBoot();

        await Page.Locator("#select-all-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("all selected", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("alice,bennett,carey,dawson", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Alice"), Is.True);
        Assert.That(await Residents.IsSelected("Bennett"), Is.True);
        Assert.That(await Residents.IsSelected("Carey"), Is.True);
        Assert.That(await Residents.IsSelected("Dawson"), Is.True);

        await Page.Locator("#unselect-all-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("all unselected", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync(string.Empty, new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-state")).ToHaveTextAsync("empty", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Alice"), Is.False);
        Assert.That(await Residents.IsSelected("Bennett"), Is.False);
        Assert.That(await Residents.IsSelected("Carey"), Is.False);
        Assert.That(await Residents.IsSelected("Dawson"), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task disable_and_enable_values_update_item_state()
    {
        await NavigateAndBoot();

        await Page.Locator("#disable-carey-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("carey disabled", new() { Timeout = 5000 });
        Assert.That(await Residents.IsDisabled("Carey"), Is.True);

        await Page.Locator("#enable-carey-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("carey enabled", new() { Timeout = 5000 });
        Assert.That(await Residents.IsDisabled("Carey"), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_reads_value_payload_from_user_click()
    {
        await NavigateAndBoot();

        await Residents.ClickItem("Bennett");

        await Expect(Page.Locator("#change-value")).ToHaveTextAsync("bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-state")).ToHaveTextAsync("has values", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator")).ToHaveTextAsync("selected", new() { Timeout = 5000 });
        Assert.That(await Residents.IsSelected("Bennett"), Is.True);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_posts_value_source_to_real_endpoint()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-value-btn").ClickAsync();
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("bennett,dawson", new() { Timeout = 5000 });

        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-value")).ToHaveTextAsync("bennett,dawson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-count")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
