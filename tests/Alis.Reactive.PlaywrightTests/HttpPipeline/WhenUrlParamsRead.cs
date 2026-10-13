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

        await Expect(Page.Locator("#url-cond-meds"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task url_condition_numeric_gt_shows_prev_button()
    {
        await NavigateWithParams();

        await Expect(Page.Locator("#url-cond-prev"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The medications panel shows on both pages, so the page condition has run when it is checked.
    [Test]
    public async Task url_condition_numeric_gt_shows_prev_button_from_page_two_but_not_on_page_one()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http?tab=medications&page=1");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#url-cond-meds")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#url-cond-prev")).ToBeHiddenAsync();

        await NavigateTo("/Sandbox/HttpPipeline/Http?tab=medications&page=2");
        await Expect(Page.Locator("#url-cond-meds")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#url-cond-prev")).ToBeVisibleAsync();
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

    // An array is read the way MVC binds one from the query: every value of the key, in any case.
    [Test]
    public async Task an_array_url_param_reads_every_value_of_its_key_in_any_case()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http?wing=garden&tab=medications&WING=maple&Wing=courtyard");
        await WaitForTraceMessage("booted", 10000);

        await Expect(Page.Locator("#url-display-wings"))
            .ToHaveTextAsync("garden,maple,courtyard", new() { Timeout = 5000 });
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
    public async Task missing_url_param_takes_the_else_branch_and_shows_the_overview()
    {
        await NavigateWithoutParams();

        await Expect(Page.Locator("#url-cond-overview"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#url-cond-meds")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
