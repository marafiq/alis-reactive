namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenRouteParamsResolve : PlaywrightTestBase
{
    private Task ClickButton(string name) =>
        ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = name }));

    private async Task NavigateAndWaitForBoot()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#load-first")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

    // ── Section 15: Single Route Param ───────────────────

    [Test]
    public async Task single_route_param_resolves_to_correct_id()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Load Resident #42");

        await Expect(Page.Locator("#route-single-id"))
            .ToHaveTextAsync("42", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task single_route_param_server_echoes_name()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Load Resident #42");

        await Expect(Page.Locator("#route-single-name"))
            .ToHaveTextAsync("Resident #42", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task single_route_param_applies_success_class()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Load Resident #42");

        await Expect(Page.Locator("#route-single-id"))
            .ToHaveTextAsync("42", new() { Timeout = 5000 });
        await Expect(Page.Locator("#route-single-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task single_route_param_hides_spinner()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Load Resident #42");

        await Expect(Page.Locator("#route-single-id"))
            .ToHaveTextAsync("42", new() { Timeout = 5000 });
        await Expect(Page.Locator("#route-single-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── Section 16: Multiple Route Params ────────────────

    [Test]
    public async Task multiple_route_params_resolve_both_values()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Load Facility Resident");

        await Expect(Page.Locator("#route-multi-facility"))
            .ToHaveTextAsync("7", new() { Timeout = 5000 });
        await Expect(Page.Locator("#route-multi-resident"))
            .ToHaveTextAsync("99", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task multiple_route_params_server_echoes_compound_name()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Load Facility Resident");

        await Expect(Page.Locator("#route-multi-name"))
            .ToHaveTextAsync("Resident #99 at Facility #7", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 17: Chained with Route Params + Headers ──

    [Test]
    public async Task chained_first_hop_resolves_route_param()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Chain with Route Params");

        await Expect(Page.Locator("#route-chain-first-id"))
            .ToHaveTextAsync("42", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_second_hop_uses_previous_response_route_param()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Chain with Route Params");

        await Expect(Page.Locator("#route-chain-second-name"))
            .ToHaveTextAsync("Resident #42 at Facility #3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_response_route_param_spinner_hides_after_both()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Chain with Route Params");

        await Expect(Page.Locator("#route-chain-second-name"))
            .ToHaveTextAsync("Resident #42 at Facility #3", new() { Timeout = 5000 });
        await Expect(Page.Locator("#route-chain-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── Section 18: URI-Encoded Route Param ──────────────

    [Test]
    public async Task route_param_with_space_is_uri_encoded()
    {
        await NavigateAndWaitForBoot();

        await ClickButton("Load Resident by Name");

        // Server receives "John Doe" after URL-decoding the path segment "John%20Doe"
        await Expect(Page.Locator("#route-encoded-name"))
            .ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
