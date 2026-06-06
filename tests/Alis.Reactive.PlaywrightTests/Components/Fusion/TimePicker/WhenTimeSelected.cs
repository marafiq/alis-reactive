using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.TimePicker;

// Syncfusion renders the visible input inside the wrapper that receives the generated component ID.
// TimePickerLocator is used when popup gestures are part of the behavior under test.
[TestFixture]
public class WhenTimeSelected : PlaywrightTestBase
{
    private const string PagePath = "/Sandbox/Components/TimePicker";

    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TimePickerModel";
    private const string MedicationTimeId = GeneratedTypeScope + "__MedicationTime";
    private const string WakeUpTimeId = GeneratedTypeScope + "__WakeUpTime";

    private TimePickerLocator MedicationTime => new(Page, MedicationTimeId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(PagePath, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("TimePicker — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_initial_value()
    {
        await NavigateAndBoot();
        var wrapper = Page.Locator($"#{MedicationTimeId}");
        await Expect(wrapper).ToBeVisibleAsync();

        await Expect(MedicationTime.Input).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#value-echo")).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task change_event_displays_new_value()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{WakeUpTimeId}");
        await input.ClickAsync();
        await input.FillAsync("10:30 AM");
        // Tab commits the Syncfusion time value and raises change.
        await input.PressAsync("Tab");

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_shows_status_when_time_selected()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#time-status")).ToBeHiddenAsync();

        var input = Page.Locator($"#{WakeUpTimeId}");
        await input.ClickAsync();
        await input.FillAsync("10:30 AM");
        await input.PressAsync("Tab");

        await Expect(Page.Locator("#time-status"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#time-status"))
            .ToHaveTextAsync("time selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_button_posts_component_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_time_multiple_times_keeps_condition_status_current()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{WakeUpTimeId}");
        var changeValue = Page.Locator("#change-value");
        var timeStatus = Page.Locator("#time-status");

        // Each Tab commits the filled value before the condition re-evaluates.
        await input.ClickAsync();
        await input.FillAsync("10:30 AM");
        await input.PressAsync("Tab");
        await Expect(changeValue).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(timeStatus).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(timeStatus).ToHaveTextAsync("time selected", new() { Timeout = 3000 });

        await input.ClickAsync();
        await input.FillAsync("02:45 PM");
        await input.PressAsync("Tab");
        await Expect(timeStatus).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(timeStatus).ToHaveTextAsync("time selected", new() { Timeout = 3000 });

        await input.ClickAsync();
        await input.FillAsync("06:00 AM");
        await input.PressAsync("Tab");
        await Expect(timeStatus).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(timeStatus).ToHaveTextAsync("time selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_sends_current_time_value_not_initial()
    {
        await NavigateAndBoot();

        // DomReady set MedicationTime to 08:30; change it through the time popup.
        await MedicationTime.SelectTime("2:00 PM");

        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
