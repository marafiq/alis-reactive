namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenUrlParamsRead : PlaywrightTestBase
{
    private Task ClickButton(string name) =>
        ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = name }));

    /// <summary>Navigate WITH query params so FromUrl reads real values.</summary>
    private async Task NavigateWithParams()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http?tab=medications&facilityId=7&page=3");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#load-first")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

    /// <summary>Navigate WITHOUT query params so FromUrl returns null.</summary>
    private async Task NavigateWithoutParams()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#load-first")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

    // ── Section 19: FromUrl in Gather ────────────────────────

    [Test]
    public async Task url_param_sent_as_gather_field()
    {
        await NavigateWithParams();

        await ClickButton("Send URL Params");

        await Expect(Page.Locator("#url-gather-tab"))
            .ToHaveTextAsync("medications", new() { Timeout = 5000 });
        await Expect(Page.Locator("#url-gather-facility"))
            .ToHaveTextAsync("7", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task url_gather_applies_success_class()
    {
        await NavigateWithParams();

        await ClickButton("Send URL Params");

        await Expect(Page.Locator("#url-gather-tab"))
            .ToHaveTextAsync("medications", new() { Timeout = 5000 });
        await Expect(Page.Locator("#url-gather-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    // ── Section 20: FromUrl in Conditions (DomReady) ─────────

    [Test]
    public async Task url_condition_string_eq_shows_correct_panel()
    {
        await NavigateWithParams();

        // ?tab=medications → condition matches → panel visible
        await Expect(Page.Locator("#url-cond-meds"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task url_condition_numeric_gt_shows_prev_button()
    {
        await NavigateWithParams();

        // ?page=3 → FromUrl<int>("page").Gt(1) → true → prev visible
        await Expect(Page.Locator("#url-cond-prev"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 21: FromUrl in SetText (DomReady) ────────────

    [Test]
    public async Task url_param_displayed_in_element_text()
    {
        await NavigateWithParams();

        await Expect(Page.Locator("#url-display-tab"))
            .ToHaveTextAsync("medications", new() { Timeout = 5000 });
        await Expect(Page.Locator("#url-display-facility"))
            .ToHaveTextAsync("7", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 22: Composition ──────────────────────────────

    [Test]
    public async Task url_param_composes_route_param_resolves()
    {
        await NavigateWithParams();

        await ClickButton("Compose All Sources");

        await Expect(Page.Locator("#url-compose-name"))
            .ToHaveTextAsync("Resident #42", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task url_param_composes_header_reaches_server()
    {
        await NavigateWithParams();

        await ClickButton("Compose All Sources");

        await Expect(Page.Locator("#url-compose-tab"))
            .ToHaveTextAsync("medications", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task url_param_composes_gather_field_reaches_server()
    {
        await NavigateWithParams();

        await ClickButton("Compose All Sources");

        await Expect(Page.Locator("#url-compose-facility"))
            .ToHaveTextAsync("7", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Missing params ───────────────────────────────────────

    [Test]
    public async Task missing_url_param_returns_null_condition_hides_panel()
    {
        await NavigateWithoutParams();

        // No ?tab → condition evaluates to false → panel stays hidden
        await Expect(Page.Locator("#url-cond-meds"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
