namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises the deterministic array-operations DSL end-to-end in the browser:
/// NativeCheckList Changed -> p.From(args, e => e.Value).Count() ->
/// p.Element("selected-count").SetText(count). Proves the array-op plan node executes
/// in the runtime against a real string[] event payload, with no plugin and no hand-written JS.
///
/// Page under test: /Sandbox/Components/ArrayOps
/// Isolated slice — touches no other vertical slice.
/// </summary>
[TestFixture]
public class WhenArrayOpsCountsSelection : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ArrayOps";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ArrayOpsModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("ArrayOps — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_count_shows_one_after_first_activity_is_checked()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{Scope}SelectedActivities_c0").ClickAsync();

        var count = Page.Locator("#selected-count");
        await Expect(count).ToHaveTextAsync("1", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_count_increments_as_more_activities_are_checked()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{Scope}SelectedActivities_c0").ClickAsync();
        await Page.Locator($"#{Scope}SelectedActivities_c1").ClickAsync();

        var count = Page.Locator("#selected-count");
        await Expect(count).ToHaveTextAsync("2", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_count_decrements_when_an_activity_is_unchecked()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{Scope}SelectedActivities_c0").ClickAsync();
        await Page.Locator($"#{Scope}SelectedActivities_c1").ClickAsync();
        await Page.Locator($"#{Scope}SelectedActivities_c0").ClickAsync();

        var count = Page.Locator("#selected-count");
        await Expect(count).ToHaveTextAsync("1", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selected_count_shows_full_total_when_all_activities_are_checked()
    {
        await NavigateAndBoot();

        for (var i = 0; i < 5; i++)
            await Page.Locator($"#{Scope}SelectedActivities_c{i}").ClickAsync();

        var count = Page.Locator("#selected-count");
        await Expect(count).ToHaveTextAsync("5", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }
}
