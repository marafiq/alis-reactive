namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

/// <summary>
/// .Finally() cleanup commands execute after HTTP success, routed errors,
/// unmatched statuses, and network failures.
/// </summary>
[TestFixture]
public class WhenFinallyExecutes : PlaywrightTestBase
{
    private Task ClickButton(string name) =>
        ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = name }));

    private async Task NavigateToHttpPage()
    {
        await NavigateToAndWaitForTextSignal(
            "/Sandbox/HttpPipeline/Http",
            "#load-first");
    }

    [Test]
    public async Task finally_hides_spinner_after_successful_save()
    {
        await NavigateToHttpPage();

        await ClickButton("Finally OK");

        await Expect(Page.Locator("#finally-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#finally-result")).ToHaveTextAsync("Saved successfully!");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finally_hides_spinner_after_server_500()
    {
        await NavigateToHttpPage();

        await ClickButton("Finally 500");

        await Expect(Page.Locator("#finally-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#finally-result"))
            .ToContainTextAsync("Server error (500)");
        AssertNoConsoleErrorsExcept("500", "http");
    }

    [Test]
    public async Task finally_hides_spinner_when_no_error_route_matches()
    {
        await NavigateToHttpPage();

        await Page.RouteAsync("**/Sandbox/HttpPipeline/Http/FlakyEndpoint", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 502,
                ContentType = "application/json",
                Body = "{\"error\": \"Bad Gateway\"}"
            });
        });

        await ClickButton("Finally OK");

        await Expect(Page.Locator("#finally-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("502", "http");
    }

    [Test]
    public async Task finally_hides_spinner_on_network_error()
    {
        await NavigateToHttpPage();

        await Page.RouteAsync("**/Sandbox/HttpPipeline/Http/FlakyEndpoint", async route =>
        {
            await route.AbortAsync("connectionrefused");
        });

        await ClickButton("Finally OK");

        await Expect(Page.Locator("#finally-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("network", "TypeError", "http", "ERR_");
    }
}
