namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenCustomHeadersSent : PlaywrightTestBase
{
    private Task ClickButton(string name) =>
        ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = name }));

    private async Task NavigateAndWaitForDomReadyLoad()
    {
        await NavigateTo("/Sandbox/HttpPipeline/Http");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#load-first")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

    [Test]
    public async Task literal_headers_arrive_at_server()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Send with Headers");

        await Expect(Page.Locator("#header-api-version"))
            .ToHaveTextAsync("2024-01-15", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-request-id"))
            .ToHaveTextAsync("test-header-123", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-tenant-id"))
            .ToHaveTextAsync("facility-42", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task literal_headers_apply_success_class()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Send with Headers");

        await Expect(Page.Locator("#header-api-version"))
            .ToHaveTextAsync("2024-01-15", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task literal_headers_hide_spinner()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Send with Headers");

        await Expect(Page.Locator("#header-api-version"))
            .ToHaveTextAsync("2024-01-15", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_first_request_carries_its_headers()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Chain with Headers");

        await Expect(Page.Locator("#header-chain-first-version"))
            .ToHaveTextAsync("chain-v1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-chain-first-id"))
            .ToHaveTextAsync("chain-req-1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_second_request_waits_for_first_response_and_carries_different_headers()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Chain with Headers");

        await Expect(Page.Locator("#header-chain-second-version"))
            .ToHaveTextAsync("chain-v2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-chain-second-tenant"))
            .ToHaveTextAsync("chain-tenant-99", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chained_headers_spinner_hides_after_both()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Chain with Headers");

        await Expect(Page.Locator("#header-chain-second-version"))
            .ToHaveTextAsync("chain-v2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-chain-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_request_a_carries_its_headers()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Parallel with Headers");

        await Expect(Page.Locator("#header-parallel-a-version"))
            .ToHaveTextAsync("parallel-a", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-parallel-a-id"))
            .ToHaveTextAsync("parallel-req-a", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_request_b_carries_its_headers()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Parallel with Headers");

        await Expect(Page.Locator("#header-parallel-b-version"))
            .ToHaveTextAsync("parallel-b", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-parallel-b-tenant"))
            .ToHaveTextAsync("parallel-tenant-b", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_headers_all_settled_fires()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Parallel with Headers");

        await Expect(Page.Locator("#header-parallel-all"))
            .ToHaveTextAsync("Both parallel requests with headers completed!", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-parallel-all")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parallel_headers_spinner_hides_after_all_settled()
    {
        await NavigateAndWaitForDomReadyLoad();

        await ClickButton("Parallel with Headers");

        await Expect(Page.Locator("#header-parallel-all"))
            .ToHaveTextAsync("Both parallel requests with headers completed!", new() { Timeout = 5000 });
        await Expect(Page.Locator("#header-parallel-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
