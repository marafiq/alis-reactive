namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenArrayPluginManipulates : PlaywrightTestBase
{
    private async Task NavigateAndWaitForDomReady()
    {
        await NavigateTo("/Sandbox/Plugins/ArrayManager");
        await WaitForTraceMessage("booted", 10000);
        // Wait for DomReady GET to complete — total count populates
        await Expect(Page.Locator("#arr-total")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

    // ── DomReady: array operations on server data ──────────

    [Test]
    public async Task count_shows_total_residents()
    {
        await NavigateAndWaitForDomReady();

        await Expect(Page.Locator("#arr-total"))
            .ToHaveTextAsync("5", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pluck_shows_first_resident_name()
    {
        await NavigateAndWaitForDomReady();

        await Expect(Page.Locator("#arr-first-name"))
            .ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_plus_count_shows_active_residents()
    {
        await NavigateAndWaitForDomReady();

        // 3 active residents: John Doe, Jane Smith, Alice Brown
        await Expect(Page.Locator("#arr-active-count"))
            .ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sum_shows_total_age()
    {
        await NavigateAndWaitForDomReady();

        // 82 + 75 + 68 + 91 + 77 = 393
        await Expect(Page.Locator("#arr-total-age"))
            .ToHaveTextAsync("393", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task some_condition_shows_no_critical_message()
    {
        await NavigateAndWaitForDomReady();

        // No resident has status "critical" — should show "No critical" message
        await Expect(Page.Locator("#arr-no-critical")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#arr-has-critical")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_ready_applies_success_class()
    {
        await NavigateAndWaitForDomReady();

        await Expect(Page.Locator("#arr-results")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    // ── Button: plugin values in gather + header ──────────

    [Test]
    public async Task send_button_echoes_count_value()
    {
        await NavigateAndWaitForDomReady();

        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Send to Server" }));

        await Expect(Page.Locator("#arr-echo-count"))
            .Not.ToHaveTextAsync("—", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task send_button_echoes_header_value()
    {
        await NavigateAndWaitForDomReady();

        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Send to Server" }));

        await Expect(Page.Locator("#arr-echo-header"))
            .Not.ToHaveTextAsync("—", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
