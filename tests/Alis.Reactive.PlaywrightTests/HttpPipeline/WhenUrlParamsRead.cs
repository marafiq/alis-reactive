namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenUrlParamsRead : PlaywrightTestBase
{
    private Task ClickButton(string name) =>
        ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = name }));

    private async Task NavigateWithParams()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http?tab=medications&facilityId=7&page=3&residentId=42");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#load-first")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

    private async Task NavigateWithoutParams()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#load-first")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

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

    [Test]
    public async Task url_condition_string_eq_shows_correct_panel()
    {
        await NavigateWithParams();

        // FromUrl("tab") matches medications, so the panel is visible.
        await Expect(Page.Locator("#url-cond-meds"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task url_condition_numeric_gt_shows_prev_button()
    {
        await NavigateWithParams();

        // FromUrl<int>("page").Gt(1) is true for page=3.
        await Expect(Page.Locator("#url-cond-prev"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

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

    [Test]
    public async Task url_param_sources_work_inside_chained_request_route_params()
    {
        await NavigateWithParams();

        await ClickButton("Chain URL Sources");

        await Expect(Page.Locator("#url-chain-name"))
            .ToHaveTextAsync("Resident #42 at Facility #7", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task missing_url_param_returns_null_condition_hides_panel()
    {
        await NavigateWithoutParams();

        // Missing FromUrl("tab") evaluates the condition false.
        await Expect(Page.Locator("#url-cond-meds"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
